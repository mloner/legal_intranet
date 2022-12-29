using System;

namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class SubtaskCreateRequest : TaskCreateRequest
    {
        public int? ParentTaskId { get; set; }
        public DateTime? Deadline { get; set; }
    }
}