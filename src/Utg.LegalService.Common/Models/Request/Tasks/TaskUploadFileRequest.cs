using Microsoft.AspNetCore.Http;

namespace Utg.LegalService.Common.Models.Request.Tasks;

public class TaskUploadFileRequest
{
    public int TaskId { get; set; }
    public IFormFile[] Attachments { get; set; }
}