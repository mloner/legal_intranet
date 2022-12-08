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
using Utg.LegalService.Common.Models.Client;
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
                .ToListAsync(cancellationToken);
            var existingUserProfileIds = items.Select(upa => upa.UserProfileId);
            var notExistingUserProfileIds = command.UserProfileIds.Except(existingUserProfileIds);
            if (notExistingUserProfileIds.Any())
            {
                var msg = "UserProfiles don't exist in the UserProfileAgregates table";
                _logger.LogError(msg + " Ids: {@Items}", notExistingUserProfileIds);
            }
            
            return Result<IEnumerable<UserProfileAgregateModel>>.Ok(items);
        }
        catch (Exception e)
        {
            var failMsg = "Failed to get UserProfileAgregates.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result<IEnumerable<UserProfileAgregateModel>>.Internal(failMsg);
        }
    }
}
