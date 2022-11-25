using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.GetList;

public class GetListUserProfileAgregatesCommandHandler 
    : IRequestHandler<GetListUserProfileAgregatesCommand, Result<IEnumerable<UserProfileAgregateModel>>>
{
    private readonly ILogger<GetListUserProfileAgregatesCommandHandler> _logger;
    private readonly UnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetListUserProfileAgregatesCommandHandler(
        ILogger<GetListUserProfileAgregatesCommandHandler> logger, 
        UnitOfWork uow,
        IMapper mapper)
    {
        _logger = logger;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserProfileAgregateModel>>> Handle(
        GetListUserProfileAgregatesCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await _uow.AgregateItems
                .GetQuery(x => command.UserProfileIds.Contains(x.UserProfileId), null)
                .ProjectTo<UserProfileAgregateModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var existingUserProfileIds = items.Select(upa => upa.UserProfileId);
            var notExistingUserProfileIds = command.UserProfileIds.Except(existingUserProfileIds);
            if (notExistingUserProfileIds.Any())
            {
                var msg = "UserProfiles don't exist in the UserProfileAgregates table";
                _logger.LogError(msg + " Ids: {@Items}", notExistingUserProfileIds);
                throw new Exception(msg);
            }
            
            return Result<IEnumerable<UserProfileAgregateModel>>.Ok(items);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get UserProfileAgregates. {@Command}", command);
            await _uow.RollbackTransactionAsync(cancellationToken);
            
            return Result<IEnumerable<UserProfileAgregateModel>>
                .Internal("Failed to get UserProfileAgregates.");
        }
    }
}
