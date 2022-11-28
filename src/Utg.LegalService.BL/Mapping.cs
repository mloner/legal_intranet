using Mapster;
using Utg.LegalService.BL.Features.SubTask.Create;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.Tasks;

namespace Utg.LegalService.BL;

public class Mapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Task, TaskModel>()
            .Map(src => src.Attachments, dst => dst.TaskAttachments)
            .MaxDepth(2);
        
        config.NewConfig<SubtaskCreateRequest, CreateSubtaskCommand>()
            .AfterMapping((request, command) =>
            {
                command.Attachments = request.Attachments;
            })
            .MaxDepth(2);
        
        config.NewConfig<TaskAttachment, TaskAttachmentModel>()
            .MaxDepth(2);
    }
}
