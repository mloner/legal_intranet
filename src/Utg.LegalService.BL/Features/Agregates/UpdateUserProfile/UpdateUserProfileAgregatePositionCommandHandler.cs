using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Agregates.UpdateUserProfile;

public class UpdateUserProfileAgregateCommandHandler 
    : IRequestHandler<UpdateUserProfileAgregateCommand, Result>
{
    private readonly ILogger<UpdateUserProfileAgregateCommandHandler> _logger;
    private readonly UnitOfWork _uow;

    public UpdateUserProfileAgregateCommandHandler(
        ILogger<UpdateUserProfileAgregateCommandHandler> logger, 
        UnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateUserProfileAgregateCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var data = command.EventModel.Data;
            if (command.EventModel.Type == UpdateEventType.Update)
            {
                var sdf = string.Format("No UserProfileAgregate. {@Command}", command);
                var upa = await _uow.AgregateItems
                    .GetQuery(x => x.UserProfileId == data.UserProfileId, null)
                    .FirstOrDefaultAsync(cancellationToken);
                if (upa == null)
                {
                    var message = string.Format("No UserProfileAgregate. {@Command}", command);
                    _logger.LogError(message);
                    return Result.Bad(message);
                }
                upa.UserId = data.UserId;
                upa.Status = data.Status;
                upa.Type = data.Type;
                upa.TabN = data.TabN;
                upa.DismissalDate = data.DismissalDate;
                upa.CompanyId = data.CompanyId;
                upa.CompanyName = data.CompanyName;
                upa.DepartmentId = data.DepartmentId;
                upa.DepartmentName = data.DepartmentName;
                upa.PositionId = data.PositionId;
                upa.PositionName = data.PositionName;
                upa.FullName = data.FullName;
                upa.ManagerPositionId = data.ManagerPositionId;
                upa.IsRemoved = data.IsRemoved;
                upa.HeadUserProfileId = data.HeadUserProfileId;
            }
            else if (command.EventModel.Type == UpdateEventType.Create)
            {
                var exists = await _uow.AgregateItems
                    .AnyAsync(x => x.UserProfileId == data.UserProfileId,
                        cancellationToken);
                if (!exists)
                {
                    await _uow.AgregateItems.AddAsync(new UserProfileAgregate
                    {
                        UserProfileId = data.UserProfileId,
                        UserId = data.UserId,
                        Status = data.Status,
                        Type = data.Type,
                        TabN = data.TabN,
                        DismissalDate = data.DismissalDate,
                        CompanyId = data.CompanyId,
                        CompanyName = data.CompanyName,
                        DepartmentId = data.DepartmentId,
                        DepartmentName = data.DepartmentName,
                        PositionId = data.PositionId,
                        PositionName = data.PositionName,
                        FullName = data.FullName,
                        ManagerPositionId = data.ManagerPositionId,
                        IsRemoved = data.IsRemoved,
                        HeadUserProfileId = data.HeadUserProfileId
                    }, cancellationToken);
                }
                else
                {
                    var message = string.Format("UserProfileAgregate already exists. {@Command}",
                        command);
                    _logger.LogError(message);
                    return Result.Bad(message);
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception e)
        {
            var failMsg = "Failed to update UserProfileAgregates.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
}
