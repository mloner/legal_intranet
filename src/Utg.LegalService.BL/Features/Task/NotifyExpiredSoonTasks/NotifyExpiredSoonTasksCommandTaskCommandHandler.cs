using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.Domain.Helpers;
using Utg.Common.Packages.Domain.Models.Notification;
using Utg.Common.Packages.Domain.Models.Push;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Services;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Task.NotifyExpiredSoonTasks;

public class NotifyExpiredSoonTasksCommandHandler 
    : IRequestHandler<NotifyExpiredSoonTasksCommand, Result>
{
    private readonly ILogger<NotifyExpiredSoonTasksCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _uow;
    private readonly IProductionCalendarService _productionCalendarService;
    private readonly INotificationService _notificationService;

    public NotifyExpiredSoonTasksCommandHandler(
        ILogger<NotifyExpiredSoonTasksCommandHandler> logger,
        IUnitOfWork uow, 
        IMediator mediator, 
        IProductionCalendarService productionCalendarService, 
        INotificationService notificationService)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
        _productionCalendarService = productionCalendarService;
        _notificationService = notificationService;
    }

    public async System.Threading.Tasks.Task<Result> Handle(
        NotifyExpiredSoonTasksCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            const int businessDaysBeforeEvent = 3;
            var lastDay = AddWorkingDays(
                command.DateTime,
                businessDaysBeforeEvent,
                _productionCalendarService);

            var neededTasks = await _uow.TaskRepository
                .GetQuery(x => x.DeadlineDateTime.HasValue &&
                               x.DeadlineDateTime.Value.Date == lastDay.Date &&
                               (x.Status == TaskStatus.InWork ||
                                x.Status == TaskStatus.UnderReview),
                    null)
                .ToListAsync(cancellationToken);
            var userProfileIds = neededTasks.Select(x => x.AuthorUserProfileId)
                .Union(neededTasks.Where(x => x.PerformerUserProfileId.HasValue)
                    .Select(x => x.PerformerUserProfileId.Value))
                .Distinct();
            var upas = await _uow.UserProfileAgregatesRepository
                .GetQuery(x => userProfileIds.Contains(x.UserProfileId), null)
                .ToListAsync(cancellationToken);

            var notifications = new List<NotificationModel>();
            foreach (var task in neededTasks)
            {
                var authorUpa = upas.FirstOrDefault(x => x.UserProfileId == task.AuthorUserProfileId);
                notifications.Add(new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskTaskExpiresSoon,
                    ToUserProfileId = task.AuthorUserProfileId,
                    ToUserProfileFullName = authorUpa?.FullName,
                    Date = command.DateTime,
                    Data = JsonConvert.SerializeObject(new BaseMessage()
                    {
                        Id = task.Id,
                        Text = NotificationTaskType.LegalTaskTaskExpiresSoon.GetDisplayName()
                    })
                });
                if (task.PerformerUserProfileId.HasValue)
                {
                    var performerUpa = upas.FirstOrDefault(x => x.UserProfileId == task.PerformerUserProfileId.Value);
                    notifications.Add(new NotificationModel
                    {
                        NotificationType = NotificationTaskType.LegalTaskTaskExpiresSoon,
                        ToUserProfileId = task.PerformerUserProfileId.Value,
                        ToUserProfileFullName = performerUpa?.FullName,
                        Date = command.DateTime,
                        Data = JsonConvert.SerializeObject(new BaseMessage()
                        {
                            Id = task.Id,
                            Text = NotificationTaskType.LegalTaskTaskExpiresSoon.GetDisplayName()
                        })
                    });
                }
            }
            
            _notificationService.Notify(notifications);
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            var failMsg = "Failed to notify expired soon tasks.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result.Internal(failMsg);
        }
    }
    
    public static DateTime AddWorkingDays(
        DateTime specificDate,
        int workingDaysToAdd,
        IProductionCalendarService productionCalendarService)
    {
        var areBusinessDays = productionCalendarService.AreBusinessDays(
            Enumerable.Range(1, workingDaysToAdd)
                .Select(x => specificDate.AddDays(x)));
        const int nextDateRangeSize = 3;
        var completeWeeks = workingDaysToAdd / 5;
        var date = specificDate.AddDays(completeWeeks * 7);
        workingDaysToAdd %= 5;
        for (int i = 0; i < workingDaysToAdd; i++)
        {
            date = date.AddDays(1);
            areBusinessDays = 
                FillBusinessDaysIfDateNotExist(areBusinessDays, date, 
                    nextDateRangeSize, 
                    productionCalendarService);
            while (!areBusinessDays[date])
            {
                date = date.AddDays(1);
                areBusinessDays = FillBusinessDaysIfDateNotExist(areBusinessDays, date, 
                    nextDateRangeSize,
                    productionCalendarService);
            }
        }
        return date;
    }

    private static Dictionary<DateTime, bool> FillBusinessDaysIfDateNotExist(
        Dictionary<DateTime, bool> areBusinessDays, 
        DateTime dateTime,
        int nextDateRangeSize,
        IProductionCalendarService productionCalendarService)
    {
        if (!areBusinessDays.TryGetValue(dateTime, out _))
        {
            areBusinessDays = productionCalendarService.AreBusinessDays(
                Enumerable.Range(1, nextDateRangeSize)
                    .Select(x => areBusinessDays.Last().Key.AddDays(x)));
        }

        return areBusinessDays;
    }

}
