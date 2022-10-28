using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utg.Common.Packages.Domain.Models.Notification;
using Utg.Common.Packages.Queue;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.BL.Services;

public class NotificationService : INotificationService
{
    private readonly IQueuePublisherService _queueService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IQueuePublisherService queueService,
        ILogger<NotificationService> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    public void Notify(NotificationModel notificationModel)
    {
        var queueName = _queueService.PushQueueName;
        var exchangeName = _queueService.PushExchangeName;
        var message = JsonConvert.SerializeObject(notificationModel);
        _logger.LogWarning($"[Notification before send] queueName {queueName}, exchangeName {exchangeName}, {message}");
        _queueService.Send(queueName, exchangeName, message);
        _logger.LogWarning($"[Notification after send] queueName {queueName}, exchangeName {exchangeName}, {message}");
    }

    public void Notify(IEnumerable<NotificationModel> notificationModels)
    {
        var messages = notificationModels.Select(message => JsonConvert.SerializeObject(message));
        var tempstr = string.Join(",", messages);

        var queueName = _queueService.PushQueueName;
        var exchangeName = _queueService.PushExchangeName;

        _logger.LogWarning($"[Notification before send] queueName {queueName}, exchangeName {exchangeName}, {tempstr}");
        _queueService.SendMany(queueName, exchangeName, messages);
        _logger.LogWarning($"[Notification after send] queueName {queueName}, exchangeName {exchangeName}, {tempstr}");
    }
}