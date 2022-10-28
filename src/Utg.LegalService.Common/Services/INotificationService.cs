using System.Collections.Generic;
using Utg.Common.Packages.Domain.Models.Notification;

namespace Utg.LegalService.Common.Services;

public interface INotificationService
{
    void Notify(NotificationModel notificationModel);
    void Notify(IEnumerable<NotificationModel> notificationModels);
}