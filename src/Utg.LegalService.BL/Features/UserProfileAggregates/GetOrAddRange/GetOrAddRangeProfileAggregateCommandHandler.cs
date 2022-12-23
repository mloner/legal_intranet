using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.GetOrAddRange;

public class GetOrAddRangeProfileAggregateCommandHandler :
    IRequestHandler<GetOrAddRangeProfileAggregateCommand, 
        ResultList<UserProfileAgregateModel>>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserProfilesGrpcService _userProfilesGrpcService;
    private readonly ILogger<GetOrAddRangeProfileAggregateCommandHandler> _logger;

    public GetOrAddRangeProfileAggregateCommandHandler(
        IUnitOfWork uow,
        IUserProfilesGrpcService userProfilesGrpcService,
        ILogger<GetOrAddRangeProfileAggregateCommandHandler> logger)
    {
        _uow = uow;
        _userProfilesGrpcService = userProfilesGrpcService;
        _logger = logger;
    }

    public async Task<ResultList<UserProfileAgregateModel>> Handle(GetOrAddRangeProfileAggregateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            ResultList<UserProfileAgregate> aggregatesResult;

            if (request.UserIds?.Any() == true)
                aggregatesResult = await GetByUserIds(request.UserIds, cancellationToken);
            else if (request.UserProfileIds?.Any() == true)
                aggregatesResult = await GetByUserProfileIds(request.UserProfileIds, cancellationToken);
            else if (request.TabNs?.Any() == true)
                aggregatesResult = await GetByTabNs(request.TabNs, cancellationToken);
            else
                return ResultList<UserProfileAgregateModel>.Bad("Invalid request");

            if (!aggregatesResult.Success)
                return ResultList<UserProfileAgregateModel>.Failed(aggregatesResult);

            var aggregates = aggregatesResult.Data;

            var result = aggregates?
                .Select(aggregate => aggregate.Adapt<UserProfileAgregateModel>());

            return ResultList<UserProfileAgregateModel>.Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get User Profile Aggregate");
            return ResultList<UserProfileAgregateModel>.Internal("Failed to get User Profile Aggregate");
        }
    }

    private async Task<ResultList<UserProfileAgregate>> GetByUserIds(
        List<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids?.Any() != true)
        {
            return ResultList<UserProfileAgregate>.Bad(
                $"Empty {nameof(GetOrAddRangeProfileAggregateCommand.UserIds)} list");
        }

        ids = ids.Distinct().ToList();

        Expression<Func<UserProfileAgregate, bool>> predicate = q => ids.Contains(q.UserId);

        var aggregates = await _uow.UserProfileAgregatesRepository.Get()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        var diff = ids.Except(aggregates.Select(q => q.UserId).Distinct()).ToList();

        if (!diff.Any())
            return ResultList<UserProfileAgregate>.Ok(aggregates);

        var profilesResponse = await _userProfilesGrpcService.GetUserProfilesAsync(
            new UserProfilesRequestContract
            {
                UserIds = diff,
            }, cancellationToken);

        if (!profilesResponse.Success)
            return ResultList<UserProfileAgregate>.Failed(profilesResponse);

        var incomingProfiles = profilesResponse.Data;

        var incomingProfileIds = incomingProfiles.Select(x => x.Id).ToList();
        var profStatusesResp = await _userProfilesGrpcService.GetUserProfileStatusesAsync(
            new UserProfileStatusRequestContract()
            {
                UserProfileIds = incomingProfileIds
            }, cancellationToken);
        if (!profStatusesResp.Success)
        {
            _logger.LogError($"{profStatusesResp.Message}. " +
                             $"Failed to get Profile statuses: {string.Join(", ", incomingProfileIds)}.");
        }

        incomingProfiles = incomingProfiles.Select(x =>
        {
            x.UserProfileStatuses = profStatusesResp.Data?
                .FirstOrDefault(y => y.Id == x.Id)?.UserProfileStatuses;
            
            return x;
        });

        return await AddOrUpdateAggregates(incomingProfiles, aggregates, cancellationToken);
    }

    private async Task<ResultList<UserProfileAgregate>> GetByTabNs(
        List<string> tabns,
        CancellationToken cancellationToken)
    {
        if (tabns?.Any() != true)
        {
            return ResultList<UserProfileAgregate>.Bad(
                $"Empty {nameof(GetOrAddRangeProfileAggregateCommand.UserIds)} list");
        }

        tabns = tabns.Distinct().ToList();

        Expression<Func<UserProfileAgregate, bool>> predicate = q =>
            !string.IsNullOrWhiteSpace(q.TabN) && tabns.Contains(q.TabN);

        var aggregates = await _uow.UserProfileAgregatesRepository.Get()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        var diff = tabns.Except(
            aggregates.Where(q => !string.IsNullOrWhiteSpace(q.TabN)).Select(q => q.TabN).Distinct()
        ).ToList();

        if (!diff.Any())
            return ResultList<UserProfileAgregate>.Ok(aggregates);

        var profilesResponse = await _userProfilesGrpcService.GetUserProfilesAsync(
            new UserProfilesRequestContract
            {
                TabNs = diff,
            }, cancellationToken);

        if (!profilesResponse.Success)
            return ResultList<UserProfileAgregate>.Failed(profilesResponse);

        var incomingProfiles = profilesResponse.Data;
        
        var incomingProfileIds = incomingProfiles.Select(x => x.Id).ToList();
        var profStatusesResp = await _userProfilesGrpcService.GetUserProfileStatusesAsync(
            new UserProfileStatusRequestContract()
            {
                UserProfileIds = incomingProfileIds
            }, cancellationToken);
        if (!profStatusesResp.Success)
        {
            _logger.LogError($"{profStatusesResp.Message}. " +
                             $"Failed to get Profile statuses: {string.Join(", ", incomingProfileIds)}.");
        }
        incomingProfiles = incomingProfiles.Select(x =>
        {
            x.UserProfileStatuses = profStatusesResp.Data?
                .FirstOrDefault(y => y.Id == x.Id)?.UserProfileStatuses;
            
            return x;
        });
        

        return await AddOrUpdateAggregates(incomingProfiles, aggregates, cancellationToken);
    }

    private async Task<ResultList<UserProfileAgregate>> GetByUserProfileIds(
        List<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids?.Any() != true)
        {
            return ResultList<UserProfileAgregate>.Bad(
                $"Empty {nameof(GetOrAddRangeProfileAggregateCommand.UserIds)} list");
        }

        ids = ids.Distinct().ToList();

        Expression<Func<UserProfileAgregate, bool>> predicate = q => ids.Contains(q.UserProfileId);

        var aggregates = await _uow.UserProfileAgregatesRepository.Get()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        var diff = ids.Except(
            aggregates.Select(q => q.UserProfileId).Distinct()
        ).ToList();

        if (!diff.Any())
            return ResultList<UserProfileAgregate>.Ok(aggregates);

        var profilesResponse = await _userProfilesGrpcService.GetUserProfilesAsync(
            new UserProfilesRequestContract
            {
                UserProfileIds = diff,
            }, cancellationToken);

        if (!profilesResponse.Success)
        {
            if (profilesResponse.StatusCode != (int) HttpStatusCode.NotFound)
            {
                return ResultList<UserProfileAgregate>.Failed(profilesResponse);
            }
            else
            {
                _logger.LogWarning($"Can't find userprofiles " +
                                   $"{string.Join(", ", diff)}");
                return ResultList<UserProfileAgregate>.Ok(aggregates);
            }
        }

        
        var incomingProfiles = profilesResponse.Data;
        var incomingProfileIds = incomingProfiles.Select(x => x.Id).ToList();
        var profStatusesResp = await _userProfilesGrpcService.GetUserProfileStatusesAsync(
            new UserProfileStatusRequestContract()
            {
                UserProfileIds = incomingProfileIds
            }, cancellationToken);
        if (!profStatusesResp.Success)
        {
            _logger.LogError($"{profStatusesResp.Message}. " +
                             $"Failed to get Profile statuses: {string.Join(", ", incomingProfileIds)}.");
        }
        incomingProfiles = incomingProfiles.Select(x =>
        {
            x.UserProfileStatuses = profStatusesResp.Data?
                .FirstOrDefault(y => y.Id == x.Id)?.UserProfileStatuses;
            
            return x;
        });

        return await AddOrUpdateAggregates(incomingProfiles, aggregates, cancellationToken);
    }

    private async Task<ResultList<UserProfileAgregate>> AddOrUpdateAggregates(
        IEnumerable<UserProfileViewModelContract> userProfileContracts,
        IEnumerable<UserProfileAgregate> existModels,
        CancellationToken cancellationToken)
    {
        try
        {
            Expression<Func<UserProfileAgregate, bool>> predicate = q => userProfileContracts
                .Select(c => c.Id)
                .Contains(q.UserProfileId);

            var aggregates = await _uow.UserProfileAgregatesRepository.Get()
                .Where(predicate)
                .ToListAsync(cancellationToken);

            if (userProfileContracts?.Any() != true)
                return ResultList<UserProfileAgregate>.Bad("Empty data for update");

            var modelsToAdd = new List<UserProfileAgregate>();

            foreach (var userProfileContract in userProfileContracts)
            {
                var model = aggregates.FirstOrDefault(q => q.UserProfileId == userProfileContract.Id);

                if (model == null)
                {
                    model = new UserProfileAgregate();
                    userProfileContract.Adapt(model);
                    modelsToAdd.Add(model);
                }
                else
                {
                    userProfileContract.Adapt(model);
                }
            }

            if (modelsToAdd.Any())
                await _uow.UserProfileAgregatesRepository.AddUserProfilesAsync(modelsToAdd, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            aggregates.AddRange(modelsToAdd);
            aggregates.AddRange(existModels);
            return ResultList<UserProfileAgregate>.Ok(aggregates);
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to update models from contracts";
            _logger.LogError(e, "{@Msg}", failMsg);
            
            return ResultList<UserProfileAgregate>.Internal(failMsg);
        }
    }
}
