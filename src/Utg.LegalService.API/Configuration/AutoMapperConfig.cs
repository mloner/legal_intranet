using System;
using AutoMapper;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;

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
                ;
        }
	}
}
