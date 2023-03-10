using System;
using AutoMapper;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.CompanyUpdate;
using Utg.Api.Common.Models.UpdateModels.DepartmentUpdate;
using Utg.Api.Common.Models.UpdateModels.PositionUpdate;
using Utg.Api.Common.Models.UpdateModels.UserProfileUpdate;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdatePosition;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.TaskComments;
using Utg.LegalService.Common.Models.Request.Tasks;

namespace Utg.LegalService.API.Configuration
{
    public static class AutoMapperConfig
    {
        public static void ConfigureMappings(IMapperConfigurationExpression config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            
            config.CreateMap<TaskCreateRequest, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(x => x.Description))
                .ForMember(dest => dest.AuthorUserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorFullName, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.PerformerUserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.PerformerFullName, opt => opt.Ignore())
                .ForMember(dest => dest.DeadlineDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastChangeDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.AccessRights, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTaskId, opt => opt.Ignore())
                .ForMember(dest => dest.Subtasks, opt => opt.Ignore())
                ;
            
            config.CreateMap<Task, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(x => x.Description))
                .ForMember(dest => dest.AuthorUserProfileId, opt => opt.MapFrom(x => x.AuthorUserProfileId))
                .ForMember(dest => dest.AuthorFullName, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDateTime, opt => opt.MapFrom(x => x.CreationDateTime))
                .ForMember(dest => dest.PerformerUserProfileId, opt => opt.MapFrom(x => x.PerformerUserProfileId))
                .ForMember(dest => dest.PerformerFullName, opt => opt.Ignore())
                .ForMember(dest => dest.DeadlineDateTime, opt => opt.MapFrom(x => x.DeadlineDateTime))
                .ForMember(dest => dest.LastChangeDateTime, opt => opt.MapFrom(x => x.LastChangeDateTime))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(x => x.TaskAttachments))
                .ForMember(dest => dest.AccessRights, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.Subtasks, opt => opt.Ignore())
                ;
            
            config.CreateMap<TaskModel, Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(x => x.Description))
                .ForMember(dest => dest.AuthorUserProfileId, opt => opt.MapFrom(x => x.AuthorUserProfileId))
                .ForMember(dest => dest.CreationDateTime, opt => opt.MapFrom(x => x.CreationDateTime))
                .ForMember(dest => dest.PerformerUserProfileId, opt => opt.MapFrom(x => x.PerformerUserProfileId))
                .ForMember(dest => dest.DeadlineDateTime, opt => opt.MapFrom(x => x.DeadlineDateTime))
                .ForMember(dest => dest.LastChangeDateTime, opt => opt.MapFrom(x => x.LastChangeDateTime))
                .ForMember(dest => dest.TaskAttachments, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTaskId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.TaskChangeHistories, opt => opt.Ignore())
                ;
            
            config.CreateMap<TaskCreateRequest, Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(x => x.Description))
                .ForMember(dest => dest.AuthorUserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.PerformerUserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.DeadlineDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastChangeDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAttachments, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTaskId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.TaskChangeHistories, opt => opt.Ignore())
                ;

            config.CreateMap<TaskAttachmentModel, TaskAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(x => x.FileId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(x => x.FileName))
                .ForMember(dest => dest.FileSizeInBytes, opt => opt.MapFrom(x => x.FileSizeInBytes))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfileId, opt => opt.MapFrom(x => x.UserProfileId))
                ;
            
            config.CreateMap<TaskAttachment, TaskAttachmentModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(x => x.FileId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(x => x.FileName))
                .ForMember(dest => dest.FileSizeInBytes, opt => opt.MapFrom(x => x.FileSizeInBytes))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.Task, opt => opt.MapFrom(x => x.Task))
                .ForMember(dest => dest.Bytes, opt => opt.Ignore())
                .ForMember(dest => dest.AccessRights, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfileId, opt => opt.MapFrom(x => x.UserProfileId))
                .ForMember(dest => dest.Url, opt => opt.Ignore())
                ;
            
            config.CreateMap<TaskComment, TaskCommentModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.UserProfileId, opt => opt.MapFrom(x => x.UserProfileId))
                .ForMember(dest => dest.UserProfileFullName, opt => opt.Ignore())
                .ForMember(dest => dest.DateTime, opt => opt.MapFrom(x => x.DateTime))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(x => x.Text))
                ;
            
            config.CreateMap<TaskCommentModel, TaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.UserProfileId, opt => opt.MapFrom(x => x.UserProfileId))
                .ForMember(dest => dest.DateTime, opt => opt.MapFrom(x => x.DateTime))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(x => x.Text))
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForAllMembers(x => x.Ignore())
                ;
            
            config.CreateMap<TaskCommentCreateRequest, TaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.UserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.DateTime, opt => opt.Ignore())
                .ForMember(dest => dest.Text, opt => opt.MapFrom(x => x.Text))
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.Created, src => src.Ignore())
                .ForMember(dest => dest.Updated, src => src.Ignore())
                ;
            
            #region agregates

            config.CreateMap<UserProfileApiModel, UserProfileAgregate>()
                .ForMember(dst => dst.Id, src => src.Ignore())
                .ForMember(dst => dst.UserProfileId, src => src.MapFrom(x => x.Id))
                .ForMember(dst => dst.UserId, src => src.MapFrom(x => x.UserId))
                .ForMember(dst => dst.Status, src => src.MapFrom(x => x.Status))
                .ForMember(dst => dst.Type, src => src.MapFrom(x => x.Type))
                .ForMember(dst => dst.TabN, src => src.MapFrom(x => x.TabN))
                .ForMember(dst => dst.CompanyId, src => src.MapFrom(x => x.CompanyId))
                .ForMember(dst => dst.CompanyName, src => src.MapFrom(x => x.CompanyName))
                .ForMember(dst => dst.DepartmentId, src => src.MapFrom(x => x.DepartmentId))
                .ForMember(dst => dst.DepartmentName, src => src.MapFrom(x => x.DepartmentName))
                .ForMember(dst => dst.PositionId, src => src.MapFrom(x => x.PositionId))
                .ForMember(dst => dst.PositionName, src => src.MapFrom(x => x.PositionName))
                .ForMember(dst => dst.FullName, src => src.MapFrom(x => x.FullName))
                .ForMember(dst => dst.IsRemoved, src => src.MapFrom(x => x.IsRemoved))
                .ForMember(dst => dst.DismissalDate, src => src.MapFrom(x => x.DismissalDate != null ?  x.DismissalDate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dst => dst.Created, src => src.Ignore())
                .ForMember(dst => dst.Updated, src => src.Ignore())
                ;

            config.CreateMap<UserProfileAgregate, UserProfileAgregateModel>()
                .ForMember(dst => dst.Id, src => src.MapFrom(x => x.Id))
                .ForMember(dst => dst.UserProfileId, src => src.MapFrom(x => x.UserProfileId))
                .ForMember(dst => dst.UserId, src => src.MapFrom(x => x.UserId))
                .ForMember(dst => dst.Status, src => src.MapFrom(x => x.Status))
                .ForMember(dst => dst.Type, src => src.MapFrom(x => x.Type))
                .ForMember(dst => dst.TabN, src => src.MapFrom(x => x.TabN))
                .ForMember(dst => dst.CompanyId, src => src.MapFrom(x => x.CompanyId))
                .ForMember(dst => dst.CompanyName, src => src.MapFrom(x => x.CompanyName))
                .ForMember(dst => dst.DepartmentId, src => src.MapFrom(x => x.DepartmentId))
                .ForMember(dst => dst.DepartmentName, src => src.MapFrom(x => x.DepartmentName))
                .ForMember(dst => dst.PositionId, src => src.MapFrom(x => x.PositionId))
                .ForMember(dst => dst.PositionName, src => src.MapFrom(x => x.PositionName))
                .ForMember(dst => dst.FullName, src => src.MapFrom(x => x.FullName))
                .ForMember(dst => dst.IsRemoved, src => src.MapFrom(x => x.IsRemoved))
                .ForMember(dst => dst.DismissalDate, src => src.MapFrom(x => x.DismissalDate))
                ;
            
            config.CreateMap<UpdateEvent<PositionUpdateEventModel>, UpdateUserProfileAgregatePositionCommand>()
                .ForMember(dst => dst.EventModel, src => src.MapFrom(x => x))
                ;
            config.CreateMap<UpdateEvent<CompanyUpdateEventModel>, UpdateUserProfileAgregateCompanyCommand>()
                .ForMember(dst => dst.EventModel, src => src.MapFrom(x => x))
                ;
            config.CreateMap<UpdateEvent<DepartmentUpdateEventModel>, UpdateUserProfileAgregateDepartmentCommand>()
                .ForMember(dst => dst.EventModel, src => src.MapFrom(x => x))
                ;
            config.CreateMap<UpdateEvent<UserProfileUpdateEventModel>, UpdateUserProfileAgregateCommand>()
                .ForMember(dst => dst.EventModel, src => src.MapFrom(x => x))
                ;
            
            #endregion
        }
	}
}
