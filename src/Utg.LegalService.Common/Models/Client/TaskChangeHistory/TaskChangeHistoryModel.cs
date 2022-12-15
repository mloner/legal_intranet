using System;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Client.TaskChangeHistory;

public class TaskChangeHistoryModel
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime DateTime { get; set; }
    public int UserProfileId { get; set; }
    public string UserProfileFullName { get; set; }
    public HistoryAction HistoryAction { get; set; }
    public string HistoryActionName => HistoryAction.GetDisplayName();
    public TaskStatus TaskStatus { get; set; }
    public string TaskStatusName => TaskStatus.GetDisplayName();
}