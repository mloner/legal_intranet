using System.Collections.Generic;
using Mapster;
using Utg.Common.Extensions.Helpers;
using Utg.Common.Models.PaginationRequest;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.BL.Features.SubTask.Create;
using Utg.LegalService.BL.Features.Task.GetPage;
using Utg.LegalService.BL.Features.TaskChangeHistory.GetPage;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.TaskChangeHistory;
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
                command.Skip = request.Skip;
                command.Take = request.Take;
                command.Filter = new GetTaskPageCommandFilter()
                {
                    Search = request.Search,
                    Statuses = request.Statuses,
                    AuthorUserProfileIds = request.AuthorUserProfileIds,
                    MoveToWorkDateTimeFrom = request.MoveToWorkDateTimeFrom,
                    MoveToWorkDateTimeTo = request.MoveToWorkDateTimeTo
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
        
        config.NewConfig<GetTaskPageReportRequest, GetTaskPageCommand>()
            .AfterMapping((request, command) =>
            {
                command.Filter = new GetTaskPageCommandFilter()
                {
                    Search = request.Search,
                    Statuses = request.Statuses,
                    AuthorUserProfileIds = request.AuthorUserProfileIds,
                };
                command.SortBy = request.SortBy;
                command.SortDirection = request.SortDirection;
            })
            .MaxDepth(2);
        
        config.NewConfig<TaskChangeHistory, TaskChangeHistoryModel>()
            .MaxDepth(2);
        
        config.NewConfig<GetTaskChangeHistoryPageRequest, GetTaskChangeHistoryPageCommand>()
            .AfterMapping((request, command) =>
            {
                PageCalculateHelper.PageIndexAndSize(request, command);
                command.Filter = new GetTaskChangeHistoryPageCommandFilter()
                {
                    TaskId = request.TaskId
                };
            })
            .MaxDepth(2);
    }
}
