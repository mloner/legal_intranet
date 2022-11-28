using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.UpdateDepartment;

public class UpdateUserProfileAgregateDepartmentCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregateDepartmentCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregateDepartmentCommandHandler> _logger;
    private readonly UnitOfWork _uow;

    public UpdateUserProfileAgregateDepartmentCommandHandler(
        ILogger<UpdateUserProfileAgregateDepartmentCommandHandler> logger, 
        UnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateUserProfileAgregateDepartmentCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var data = command.EventModel.Data;
            if (command.EventModel.Type == UpdateEventType.Update)
            {
                var upas = _uow.AgregateItems.GetQuery(x => x.DepartmentId == data.Id, null);
                foreach (var upa in upas)
                {
                    upa.DepartmentName = data.Name;
                }
                await _uow.SaveChangesAsync(cancellationToken);
            }
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update UserProfileAgregate departments. {@Command}", command);
            await _uow.RollbackTransactionAsync(cancellationToken);
            
            return Result.Internal("Failed to update UserProfileAgregate departments.");
        }
    }
}
