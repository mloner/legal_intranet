using System;
using Microsoft.AspNetCore.Http;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class SubtaskCreateRequest : TaskCreateRequest
    {
        public int? ParentTaskId { get; set; }
        public DateTime? Deadline { get; set; }
    }
}