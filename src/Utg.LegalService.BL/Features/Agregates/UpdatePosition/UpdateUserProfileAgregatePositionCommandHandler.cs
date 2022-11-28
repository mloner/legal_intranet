using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.BL.Features.Agregates.Fill;
using Utg.LegalService.Common.Models.Domain;
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
                await _uow.SaveChangesAsync();
            }
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update UserProfileAgregate positions. {@Command}", command);
            await _uow.RollbackTransactionAsync(cancellationToken);
            
            return Result.Internal("Failed to update UserProfileAgregate positions.");
        }
    }
}
