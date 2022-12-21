using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Api.Common.Models.Contracts.UserProfile;
using Utg.Api.Common.Models.Contracts.UserProfileStatus;
using Utg.Api.CurrentUserService.Client;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.GetOrAdd;

public class GetOrAddProfileAggregateCommandHandler :
    IRequestHandler<GetOrAddProfileAggregateCommand, Result<UserProfileAgregateModel>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly IUserProfilesGrpcService _userProfilesGrpcService;
    private readonly ILogger<GetOrAddProfileAggregateCommandHandler> _logger;

    public GetOrAddProfileAggregateCommandHandler(
        IUnitOfWork uow,
        IUserProfilesGrpcService userProfilesGrpcService,
        ILogger<GetOrAddProfileAggregateCommandHandler> logger,
        IMapper mapper)
    {
        _uow = uow;
        _userProfilesGrpcService = userProfilesGrpcService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileAgregateModel>> Handle(
        GetOrAddProfileAggregateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _uow.UserProfileAgregatesRepository
                .GetUserProfileAgregateByUserProfileId(
                request.UserProfileId, cancellationToken);

            if (aggregate != null)
                return Result<UserProfileAgregateModel>.Ok(
                    _mapper.Map<UserProfileAgregateModel>(aggregate));

            var profilesResponse = await _userProfilesGrpcService.GetUserProfilesAsync(
                new UserProfilesRequestContract
                {
                    UserProfileIds = new List<int> { request.UserProfileId },
                }, cancellationToken);

            if (!profilesResponse.Success)
                return Result<UserProfileAgregateModel>.Failed(profilesResponse);

            var incomingProfile = 
                profilesResponse.Data?.FirstOrDefault(q => q.Id == request.UserProfileId);

            if (incomingProfile == null)
                return Result<UserProfileAgregateModel>.Bad(
                    $"Profile with Id: {request.UserProfileId} not found on users service");
            
            var profStatusesResp = await _userProfilesGrpcService.GetUserProfileStatusesAsync(
                new UserProfileStatusRequestContract()
                {
                    UserProfileIds = new List<int>(incomingProfile.Id)
                }, cancellationToken);
            if (!profStatusesResp.Success)
            {
                _logger.LogError($"{profStatusesResp.Message}. " +
                                $"Failed to get Profile statuses: {string.Join(", ", incomingProfile.Id)}.");
            }

            incomingProfile.UserProfileStatuses = profStatusesResp.Data?
                .FirstOrDefault(x => x.Id == incomingProfile.Id)?.UserProfileStatuses;

            aggregate = incomingProfile.Adapt<UserProfileAgregate>();

            await _uow.UserProfileAgregatesRepository.AddUserProfilesAsync(new[] { aggregate },
                cancellationToken);
            
            return Result<UserProfileAgregateModel>.Ok(
                _mapper.Map<UserProfileAgregateModel>(aggregate));
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to get User Profile Aggregate";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, request);
            
            return Result<UserProfileAgregateModel>.Internal(failMsg);
        }
    }
}
