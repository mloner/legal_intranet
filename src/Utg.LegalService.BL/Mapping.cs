using System.Collections.Generic;
using Mapster;
using Utg.Api.Common.Models.Contracts.UserProfile;
using Utg.Api.Common.Models.Contracts.UserProfileStatus;
using Utg.Common.Extensions.Helpers;
using Utg.Common.Models.PaginationRequest;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.BL.Features.SubTask.Create;
using Utg.LegalService.BL.Features.Task.GetPage;
using Utg.LegalService.BL.Features.TaskChangeHistory.GetPage;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdatePosition;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.TaskChangeHistory;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.CompanyUpdate;
using Utg.LegalService.Common.Models.UpdateModels.DepartmentUpdate;
using Utg.LegalService.Common.Models.UpdateModels.PositionUpdate;
using Utg.LegalService.Common.Models.UpdateModels.UserProfileUpdate;

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
                command.SortBy = request.SortBy;
                command.SortDirection = request.SortDirection;
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
                    MoveToWorkDateTimeFrom = request.MoveToWorkDateTimeFrom,
                    MoveToWorkDateTimeTo = request.MoveToWorkDateTimeTo
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

        #region updates

        config.NewConfig<UpdateEvent<UserProfileUpdateEventModel>, UpdateUserProfileAgregateCommand>()
            .Map(dst => dst.EventModel, src => src);
        config.NewConfig<UserProfileUpdateEventModel, UserProfileAgregate>();
        config.NewConfig<UserProfileViewModelContract, UserProfileAgregate>()
            .Ignore(dst => dst.Id)
            .Map(dst => dst.UserProfileId, src => src.Id)
            .Map(dst => dst.FullName, src => $"{src.Surname} {src.Name} {src.Patronymic}")
            ;
        config.NewConfig<UserProfileStatusContract, UserProfileStatus>();
        
        config.NewConfig<UpdateEvent<CompanyUpdateEventModel>, UpdateUserProfileAgregateCompanyCommand>()
            .Map(dst => dst.EventModel, src => src);
        config.NewConfig<UpdateEvent<DepartmentUpdateEventModel>, UpdateUserProfileAgregateDepartmentCommand>()
            .Map(dst => dst.EventModel, src => src);
        config.NewConfig<UpdateEvent<PositionUpdateEventModel>, UpdateUserProfileAgregatePositionCommand>()
            .Map(dst => dst.EventModel, src => src);

        #endregion
    }
}
