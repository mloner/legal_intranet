using System.Collections.Generic;
using System.Linq;
using Mapster;
using Utg.LegalService.BL.Features.SubTask.Create;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.Tasks;

namespace Utg.LegalService.BL;

public class Mapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Task, SubtaskModel>()
            .MaxDepth(2);
        
        config.NewConfig<SubtaskCreateRequest, CreateSubtaskCommand>()
            .MaxDepth(2);
        
        config.NewConfig<TaskAttachment, TaskAttachmentModel>()
            .MaxDepth(2);
        
        config.NewConfig<List<TaskAttachment>, List<TaskAttachmentModel>>()
            .AfterMapping((list, models) =>
            {
                models = list.Select(x => x.Adapt<TaskAttachmentModel>()).ToList();
            })
            .MaxDepth(2);
        
        TypeAdapterConfig<(SubtaskCreateRequest, AuthInfo), CreateSubtaskCommand>.NewConfig()
            .Map(dest => dest.AuthInfo, src => src.Item2)
            .Map(dest => dest.Attachments, src => src.Item1.Attachments)
            .Map(dest => dest.Description, src => src.Item1.Description)
            .Map(dest => dest.Status, src => src.Item1.Status)
            .Map(dest => dest.Type, src => src.Item1.Type)
            .Map(dest => dest.DeadLine, src => src.Item1.Deadline)
            .Map(dest => dest.ParentTaskId, src => src.Item1.ParentTaskId)
            ;
    }
}
