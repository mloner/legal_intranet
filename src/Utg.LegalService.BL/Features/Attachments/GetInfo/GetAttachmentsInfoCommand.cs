using System.Collections.Generic;
using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Attachments.GetInfo;

public class GetAttachmentsInfoCommand :  IRequest<Result<IEnumerable<TaskModel>>>
{
    public IEnumerable<TaskModel> TaskModels { get; set; }
}
