using System;
using AutoMapper;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;
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
                ;
            
            config.CreateMap<Task, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(x => x.Description))
                .ForMember(dest => dest.AuthorUserProfileId, opt => opt.MapFrom(x => x.AuthorUserProfileId))
                .ForMember(dest => dest.AuthorFullName, opt => opt.MapFrom(x => x.AuthorFullName))
                .ForMember(dest => dest.CreationDateTime, opt => opt.MapFrom(x => x.CreationDateTime))
                .ForMember(dest => dest.PerformerUserProfileId, opt => opt.MapFrom(x => x.PerformerUserProfileId))
                .ForMember(dest => dest.PerformerFullName, opt => opt.MapFrom(x => x.PerformerFullName))
                .ForMember(dest => dest.DeadlineDateTime, opt => opt.MapFrom(x => x.DeadlineDateTime))
                .ForMember(dest => dest.LastChangeDateTime, opt => opt.MapFrom(x => x.LastChangeDateTime))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(x => x.TaskAttachments))
                .ForMember(dest => dest.AccessRights, opt => opt.Ignore())
                ;
            
            config.CreateMap<TaskModel, Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(x => x.Description))
                .ForMember(dest => dest.AuthorUserProfileId, opt => opt.MapFrom(x => x.AuthorUserProfileId))
                .ForMember(dest => dest.AuthorFullName, opt => opt.MapFrom(x => x.AuthorFullName))
                .ForMember(dest => dest.CreationDateTime, opt => opt.MapFrom(x => x.CreationDateTime))
                .ForMember(dest => dest.PerformerUserProfileId, opt => opt.MapFrom(x => x.PerformerUserProfileId))
                .ForMember(dest => dest.PerformerFullName, opt => opt.MapFrom(x => x.PerformerFullName))
                .ForMember(dest => dest.DeadlineDateTime, opt => opt.MapFrom(x => x.DeadlineDateTime))
                .ForMember(dest => dest.LastChangeDateTime, opt => opt.MapFrom(x => x.LastChangeDateTime))
                .ForMember(dest => dest.TaskAttachments, opt => opt.Ignore())
                ;
            
            config.CreateMap<TaskCreateRequest, Task>()
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
                .ForMember(dest => dest.TaskAttachments, opt => opt.Ignore())
                ;

            config.CreateMap<TaskAttachmentModel, TaskAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(x => x.FileId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(x => x.FileName))
                .ForMember(dest => dest.FileSizeInBytes, opt => opt.MapFrom(x => x.FileSizeInBytes))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                ;
            
            config.CreateMap<TaskAttachment, TaskAttachmentModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(x => x.FileId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(x => x.FileName))
                .ForMember(dest => dest.FileSizeInBytes, opt => opt.MapFrom(x => x.FileSizeInBytes))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(x => x.TaskId))
                .ForMember(dest => dest.Task, opt => opt.MapFrom(x => x.Task))
                ;
        }
	}
}
