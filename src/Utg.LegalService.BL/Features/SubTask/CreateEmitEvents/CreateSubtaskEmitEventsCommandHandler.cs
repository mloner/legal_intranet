using System;
using System.Linq;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.Domain.Models.Enum;
using Utg.Common.Packages.Domain.Models.Notification;
using Utg.Common.Packages.Domain.Models.Push;
using Utg.LegalService.Common.Services;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.SubTask.CreateEmitEvents;

public class CreateSubtaskEmitEventsCommandHandler 
    : IRequestHandler<CreateSubtaskEmitEventsCommand, Result>
{
    private readonly ILogger<CreateSubtaskEmitEventsCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public CreateSubtaskEmitEventsCommandHandler(
        ILogger<CreateSubtaskEmitEventsCommandHandler> logger,
        UnitOfWork uow, 
        IMediator mediator, 
        INotificationService notificationService)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
        _notificationService = notificationService;
    }

    public async System.Threading.Tasks.Task<Result> Handle(
        CreateSubtaskEmitEventsCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            if (command.AuthInfo.Roles.Contains((int) Role.LegalHead) &&
                command.Task.PerformerUserProfileId.HasValue)
            {
                var notification = new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskCreated,
                    ToUserProfileId = command.Task.PerformerUserProfileId.Value,
                    ToUserProfileFullName = command.Task.PerformerFullName,
                    Date = now,
                    Data = JsonConvert.SerializeObject(
                        new BaseMessage
                        {
                            Id = command.Task.Id,
                            Text = $"Создана задача"
                        })
                };
            
                _notificationService.Notify(notification);
            }
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create subtask emit events. {@Command}", command);
            await _uow.RollbackTransactionAsync(cancellationToken);
            
            return Result.Internal("Failed to create subtask emit events.");
        }
    }

}
