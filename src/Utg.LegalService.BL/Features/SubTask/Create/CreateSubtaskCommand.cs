using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.SubTask.Create;

public class CreateSubtaskCommand :  IRequest<Result<TaskModel>>
{
    public int ParentTaskId { get; set; }
    
    public TaskStatus Status { get; set; }
    public TaskType Type { get; set; }
    public string Description { get; set; }
    public DateTime? DeadlineDateTime { get; set; }
        
    public IEnumerable<IFormFile> Attachments { get; set; }
    
    public AuthInfo AuthInfo { get; set; }
}
