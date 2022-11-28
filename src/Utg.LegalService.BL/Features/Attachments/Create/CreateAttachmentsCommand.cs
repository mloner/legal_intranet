using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;

namespace Utg.LegalService.BL.Features.Attachments.Create;

public class CreateAttachmentsCommand :  IRequest<Result<IEnumerable<TaskAttachmentModel>>>
{ 
    public int TaskId { get; set; }
    public IEnumerable<IFormFile> Attachments { get; set; }
    public AuthInfo AuthInfo { get; set; }
}
