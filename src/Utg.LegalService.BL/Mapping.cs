using System.Collections.Generic;
using Mapster;
using Utg.Common.Extensions.Helpers;
using Utg.Common.Models.PagedRequest;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.BL.Features.SubTask.Create;
using Utg.LegalService.BL.Features.Task.GetPage;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Comment;
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
        
        config.NewConfig<TaskComment, TaskCommentModel>()
            .MaxDepth(2);
        
        config.NewConfig<GetTaskPageRequest, GetTaskPageCommand>()
            .AfterMapping((request, command) =>
            {
                PageCalculateHelper.PageIndexAndSize(request, command);
                command.Filter = new GetTaskPageCommandFilter()
                {
                    Search = request.Search,
                    Statuses = request.Statuses,
                    AuthorUserProfileIds = request.AuthorUserProfileIds
                };
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    command.ListSort = new List<SortDescriptor>()
                    {
                        new(request.SortBy, 
                            EnumExtensions.GetEnumValue<EnumSortDirection>(request.SortDirection))
                    };
                }
            })
            .MaxDepth(2);
    }
}
