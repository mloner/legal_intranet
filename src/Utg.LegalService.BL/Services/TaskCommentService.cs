using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.TaskComments;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.BL.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IMapper _mapper;
        private readonly IAgregateService _agregateService;

        public TaskCommentService(
            IMapper mapper,
            ITaskCommentRepository taskCommentRepository,
            IAgregateService agregateService)
        {
            _mapper = mapper;
            _taskCommentRepository = taskCommentRepository;
            _agregateService = agregateService;
        }

        public async Task<List<TaskCommentModel>> GetByTaskId(int taskId)
        {
            var models = await _taskCommentRepository.Get()
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .ProjectTo<TaskCommentModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var userProfileIds = models.Select(x => x.UserProfileId);
            var userProfiles = await _agregateService.GetUserProfiles(userProfileIds);
            models = models.Select(m =>
            {
                var userProfile = userProfiles.FirstOrDefault(x => x.UserProfileId == m.UserProfileId);
                if (userProfile != null)
                {
                    m.UserProfileFullName = userProfile.FullName;
                }
                return m;
            }).ToList();

            return models;
        }

        public async Task CreateTaskComment(TaskCommentCreateRequest request,
            AuthInfo authInfo)
        {
            var entity = _mapper.Map<TaskComment>(request);

            entity.DateTime = DateTime.UtcNow;
            entity.UserProfileId = authInfo.UserProfileId;

            await _taskCommentRepository.CreateComment(entity);
        }
    }
}