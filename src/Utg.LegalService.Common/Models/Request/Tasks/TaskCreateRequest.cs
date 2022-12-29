using Microsoft.AspNetCore.Http;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class TaskCreateRequest
    {
        public TaskStatus Status { get; set; }
        public TaskType Type { get; set; }
        public string Description { get; set; }
        
        public IFormFile[] Attachments { get; set; }
    }
}