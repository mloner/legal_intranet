using System.Collections.Generic;
using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Attachment;

namespace Utg.LegalService.BL.Features.Attachments.Delete;

public class DeleteAttachmentFilesCommand :  IRequest<Result>
{ 
    public IEnumerable<TaskAttachmentModel> Attachments { get; set; }
}
