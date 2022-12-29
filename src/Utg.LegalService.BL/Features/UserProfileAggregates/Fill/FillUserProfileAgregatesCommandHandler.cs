using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Api.Common.Models.Contracts.UserProfile;
using Utg.Api.Common.Models.Contracts.UserProfileStatus;
using Utg.Api.CurrentUserService.Client;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.Fill;

public class FillUserProfileAgregatesCommandHandler 
    : IRequestHandler<FillUserProfileAgregatesCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<FillUserProfileAgregatesCommandHandler> _logger;
    private readonly IUserProfilesGrpcService _userProfilesGrpcService;
    private readonly ITaskAttachmentRepository _taskAttachmentRepository;
    private readonly IUserProfileAgregateRepository _userProfileAgregateRepository;
    private readonly ITaskChangeHistoryRepository _taskChangeHistoryRepository;
    private readonly ITaskCommentRepository _taskCommentRepository;
    private readonly ITaskRepository _taskRepository;


    public FillUserProfileAgregatesCommandHandler(
        ILogger<FillUserProfileAgregatesCommandHandler> logger, 
        IUnitOfWork uow,
        IUserProfileAgregateRepository userProfileAgregateRepository, 
        ITaskAttachmentRepository taskAttachmentRepository,
        ITaskChangeHistoryRepository taskChangeHistoryRepository,
        ITaskCommentRepository taskCommentRepository,
        ITaskRepository taskRepository, 
        IUserProfilesGrpcService userProfilesGrpcService)
    {
        _logger = logger;
        _uow = uow;
        _userProfileAgregateRepository = userProfileAgregateRepository;
        _taskAttachmentRepository = taskAttachmentRepository;
        _taskChangeHistoryRepository = taskChangeHistoryRepository;
        _taskCommentRepository = taskCommentRepository;
        _taskRepository = taskRepository;
        _userProfilesGrpcService = userProfilesGrpcService;
    }

    public async Task<Result> Handle(FillUserProfileAgregatesCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var userProfileIds = new List<int>();
            var needToAddUserProfileIds = new List<int>();
            var needToUpdateUserProfileIds = new List<int>();
            Queue<int> idsQueue;

            #region get iserProfileIds from db

            var candidatesUserProfileIds = await _taskAttachmentRepository
                .Get()
                .Where(q => q.UserProfileId.HasValue)
                .Select(q => q.UserProfileId.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
            userProfileIds.AddRange(candidatesUserProfileIds);
            
            var exportReqUserProfIds = await _taskChangeHistoryRepository
                .GetQuery(x => true, null)
                .Select(q => q.UserProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);
            userProfileIds.AddRange(exportReqUserProfIds);
            
            var journalUsProfIds = await _taskCommentRepository
                .Get()
                .Select(q => q.UserProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);
            userProfileIds.AddRange(journalUsProfIds);
            
            var learnHistUserProfIds = await _taskRepository
                .GetQuery(x => true, null)
                .Select(q => q.AuthorUserProfileId)
                .Union(_taskRepository.GetQuery(x => true, null)
                    .Where(x => x.PerformerUserProfileId.HasValue)
                    .Select(x => x.PerformerUserProfileId.Value)
                    .Distinct())
                .Distinct()
                .ToListAsync(cancellationToken);
            userProfileIds.AddRange(learnHistUserProfIds);

            #endregion
            
            userProfileIds = userProfileIds
                .Distinct()
                .Where(x => x != 0)
                .ToList();
            
            foreach (var userProfileId in userProfileIds)
            {
                var exist = await _userProfileAgregateRepository.AnyAsync(
                    x => x.UserProfileId == userProfileId, cancellationToken);

                if (exist)
                {
                    if (command.Full)
                    {
                        needToUpdateUserProfileIds.Add(userProfileId);
                    }
                    continue;
                }

                needToAddUserProfileIds.Add(userProfileId);
            }
            
            
            //add
            idsQueue = new Queue<int>(needToAddUserProfileIds);
            while (idsQueue.Any())
            {
                var idsPart = DequeueChunk(idsQueue, 50).ToList();
                var needToAddProfilesResponse = await _userProfilesGrpcService.GetUserProfilesAsync(
                    new UserProfilesRequestContract
                    {
                        UserProfileIds = idsPart
                    },cancellationToken);
                if (!needToAddProfilesResponse.Success)
                {
                    _logger.LogError($"{needToAddProfilesResponse.Message}. " +
                                     $"Failed to get Profiles: {string.Join(", ", idsPart)}.");
                    continue;
                }

                var needToAddProfiles = needToAddProfilesResponse.Data;
                var needToAddProfileIds = needToAddProfiles.Select(x => x.Id).ToList();
                var needToAddProfStatusesResp = 
                    await _userProfilesGrpcService.GetUserProfileStatusesAsync(
                    new UserProfileStatusRequestContract()
                    {
                        UserProfileIds = needToAddProfileIds
                    }, cancellationToken);
                if (!needToAddProfStatusesResp.Success)
                {
                    _logger.LogError($"{needToAddProfilesResponse.Message}. " +
                                    $"Failed to get Profiles: {string.Join(", ", idsPart)}.");
                    continue;
                }
                needToAddProfiles = needToAddProfiles.Select(x =>
                {
                    x.UserProfileStatuses = needToAddProfStatusesResp.Data?
                        .FirstOrDefault(y => y.Id == x.Id)?.UserProfileStatuses;
                    return x;
                });
                var aggregates = needToAddProfiles?.ToList()
                    .Select(q => q.Adapt<UserProfileAgregate>());
                
                await _userProfileAgregateRepository.AddUserProfilesAsync(aggregates,
                    cancellationToken);

                await System.Threading.Tasks.Task.Delay(1000, cancellationToken);
            }
            
            //update
            idsQueue = new Queue<int>(needToUpdateUserProfileIds);
            while (idsQueue.Any())
            {
                var idsPart = DequeueChunk(idsQueue, 50).ToList();
                var needToUpdateProfilesResponse = await _userProfilesGrpcService.GetUserProfilesAsync(
                    new UserProfilesRequestContract
                    {
                        UserProfileIds = idsPart
                    },cancellationToken);
                if (!needToUpdateProfilesResponse.Success)
                {
                    _logger.LogError($"{needToUpdateProfilesResponse.Message}. " +
                                     $"Failed to get Profiles: {string.Join(", ", idsPart)}.");
                    continue;
                }
            
                var needToUpdateProfiles = needToUpdateProfilesResponse.Data;
                var needToUpdateProfileIds = needToUpdateProfiles.Select(x => x.Id).ToList();
                var needToUpdateProfStatusesResp = 
                    await _userProfilesGrpcService.GetUserProfileStatusesAsync(
                    new UserProfileStatusRequestContract()
                    {
                        UserProfileIds = needToUpdateProfileIds
                    }, cancellationToken);
                if (!needToUpdateProfStatusesResp.Success)
                {
                    _logger.LogError($"{needToUpdateProfilesResponse.Message}. " +
                                    $"Failed to get Profiles: {string.Join(", ", idsPart)}.");
                    continue;
                }
                needToUpdateProfiles = needToUpdateProfiles.Select(x =>
                {
                    x.UserProfileStatuses = needToUpdateProfStatusesResp.Data?
                        .FirstOrDefault(y => y.Id == x.Id)?.UserProfileStatuses;
                    return x;
                });
                
                var agregates = await _userProfileAgregateRepository.
                    GetQuery(x => needToUpdateProfileIds.Contains(x.UserProfileId), null)
                    .ToListAsync(cancellationToken);
                foreach (var userProfileAgregate in agregates)
                {
                    var needToUpdateUserProfile = needToUpdateProfiles
                        .FirstOrDefault(x => x.Id == userProfileAgregate.UserProfileId);
                    if (needToUpdateUserProfile == null)
                    {
                        _logger.LogError($"{needToUpdateProfilesResponse.Message}. " +
                                         $"Failed to get Profile for update, " +
                                         $"userProfileId: {userProfileAgregate.UserProfileId}.");
                        continue;
                    }

                    needToUpdateUserProfile.Adapt(userProfileAgregate);
                }

                await System.Threading.Tasks.Task.Delay(1000, cancellationToken);
            }
            await _uow.SaveChangesAsync(cancellationToken);
            
            
            _logger.LogInformation($"{nameof(FillUserProfileAgregatesCommand)} Done");
            return Result.Ok();
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to add UserProfileAgregates.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
    private static IEnumerable<T> DequeueChunk<T>(Queue<T> queue, int chunkSize)
    {
        if (!queue.Any()) yield break;
        
        for (var i = 0; i < chunkSize && queue.Count > 0; i++)
        {
            yield return queue.Dequeue();
        }
    }
}
