using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.UpdatePosition;

public class UpdateUserProfileAgregatePositionCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregatePositionCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregatePositionCommandHandler> _logger;
    private readonly UnitOfWork _uow;

    public UpdateUserProfileAgregatePositionCommandHandler(
        ILogger<UpdateUserProfileAgregatePositionCommandHandler> logger, 
        UnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateUserProfileAgregatePositionCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var data = command.EventModel.Data;
            if (command.EventModel.Type == UpdateEventType.Update)
            {
                var upas = _uow.AgregateItems.GetQuery(x => x.PositionId == data.Id, null);
                foreach (var upa in upas)
                {
                    upa.PositionName = data.Name;
                }
                await _uow.SaveChangesAsync(cancellationToken);
            }
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            var failMsg = "Failed to update UserProfileAgregate positions.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
