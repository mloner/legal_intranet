using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.Fill;

public class FillUserProfileAgregatesCommandHandler 
    : IRequestHandler<FillUserProfileAgregatesCommand, Result>
{
    private readonly ILogger<FillUserProfileAgregatesCommandHandler> _logger;
    private readonly UnitOfWork _uow;
    private readonly IDataProxyClient _dataProxyClient;
    private readonly IMapper _mapper;

    public FillUserProfileAgregatesCommandHandler(
        ILogger<FillUserProfileAgregatesCommandHandler> logger, 
        UnitOfWork uow,
        IDataProxyClient dataProxyClient,
        IMapper mapper)
    {
        _logger = logger;
        _uow = uow;
        _dataProxyClient = dataProxyClient;
        _mapper = mapper;
    }

    public async Task<Result> Handle(FillUserProfileAgregatesCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var allUserProfiles = (await _dataProxyClient.AddressBookAsync(
                includeExternal: true,
                cancellationToken: cancellationToken)).Result;
            var allUserProfileIds = allUserProfiles.Select(x => x.Id);
            
            var existedUserProfileIds = _uow.AgregateItems
                .GetQuery(x => true, null)
                .Select(x => x.UserProfileId);

            var needToAddUserProfileIds = allUserProfileIds.Except(existedUserProfileIds);
            if (needToAddUserProfileIds.Any())
            {
                var needToAddUserProfiles = 
                    allUserProfiles.Where(x => needToAddUserProfileIds.Contains(x.Id));
                var needToAddUserProfileEntities =
                    _mapper.Map<IEnumerable<UserProfileAgregate>>(needToAddUserProfiles);
                await _uow.AgregateItems.AddRange(needToAddUserProfileEntities, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("UserProfileAgregates added {@Items}", needToAddUserProfiles); 
            }
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            var failMsg = "Failed to add UserProfileAgregates.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
