using System;
using System.ComponentModel.DataAnnotations;
using Utg.Common.Models.Domain;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Domain;

public class TaskChangeHistory : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime DateTime { get; set; }
    public int UserProfileId { get; set; }
    public HistoryAction HistoryAction { get; set; }
    public TaskStatus TaskStatus { get; set; }

    public virtual Task Task { get; set; }
}