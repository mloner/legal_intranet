using System;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class TaskUpdateMoveToInWorkRequest
    {
        public int Id { get; set; }
        public TaskStatus Status { get; set; }
        public int PerformerUserProfileId { get; set; }
        public DateTime DeadlineDateTime { get; set; }
    }
}