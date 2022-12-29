using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Common.Models;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;

public class UpdateUserProfileAgregateDepartmentCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregateDepartmentCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregateDepartmentCommandHandler> _logger;
    private readonly IUnitOfWork _uow;

    public UpdateUserProfileAgregateDepartmentCommandHandler(
        ILogger<UpdateUserProfileAgregateDepartmentCommandHandler> logger, 
        IUnitOfWork uow)
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
                var upas = _uow.UserProfileAgregatesRepository
                    .GetQuery(x => x.DepartmentId == data.Id, null);
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
            var failMsg = "Failed to update UserProfileAgregate departments.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
