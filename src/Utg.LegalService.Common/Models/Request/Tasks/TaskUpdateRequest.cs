using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class TaskUpdateRequest
    {
        public int Id { get; set; }
        public TaskStatus? Status { get; set; }
        public TaskType? Type { get; set; }
        public string Description { get; set; }
        public int? PerformerUserProfileId { get; set; }
        public DateTime? DeadlineDateTime { get; set; }
        
        public IFormFile[]? AddedAttachments { get; set; }
        public IEnumerable<int>? RemovedAttachmentIds { get; set; }
    }
}