using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.Domain.Models.Notification;
using Utg.Common.Packages.Domain.Models.Push;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Models.Request.TaskComments;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using Utg.LegalService.Dal;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.BL.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _uow;

        public TaskCommentService(
            IMapper mapper,
            ITaskCommentRepository taskCommentRepository,
            INotificationService notificationService,
            IMediator mediator, 
            IUnitOfWork uow)
        {
            _mapper = mapper;
            _taskCommentRepository = taskCommentRepository;
            _notificationService = notificationService;
            _mediator = mediator;
            _uow = uow;
        }

        public async Task<List<TaskCommentModel>> GetByTaskId(int taskId)
        {
            var models = await _taskCommentRepository.Get()
                .AsNoTracking()
                .Where(comment => comment.TaskId == taskId)
                .ProjectTo<TaskCommentModel>(_mapper.ConfigurationProvider)
                .OrderByDescending(comment => comment.DateTime)
                .ToListAsync();

            var userProfileIds = models.Select(x => x.UserProfileId);
            var upas = _uow.UserProfileAgregatesRepository
                .GetQuery(x => userProfileIds.Contains(x.UserProfileId), null);
            models = models.Select(m =>
            {
                var userProfile = upas.FirstOrDefault(x => x.UserProfileId == m.UserProfileId);
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
            var comment = await _taskCommentRepository.Get()
                .Include(x => x.Task)
                .FirstOrDefaultAsync(x => x.TaskId == entity.TaskId);
            await CreateTaskCommentEmitEvents(comment, authInfo);
        }
        
        private async Task CreateTaskCommentEmitEvents(TaskComment taskComment, AuthInfo authInfo)
        {
            var now = DateTime.UtcNow;
            var notifications = Enumerable.Empty<NotificationModel>();
            var task = taskComment.Task;

            if (authInfo.UserProfileId != task.AuthorUserProfileId)
            {
                var authorUpa =
                    await _uow.UserProfileAgregatesRepository
                        .GetQuery(x => x.UserProfileId == task.AuthorUserProfileId, null)
                        .FirstOrDefaultAsync();
                notifications = notifications.Append(new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskCommentCreated,
                    ToUserProfileId = task.AuthorUserProfileId,
                    ToUserProfileFullName = authorUpa?.FullName,
                    Date = now,
                    Data = JsonConvert.SerializeObject(
                        new BaseMessage
                        {
                            Id = task.Id,
                            Text = $"В задаче появился новый комментарий"
                        })
                });
            }
            if (task.PerformerUserProfileId.HasValue &&
                authInfo.UserProfileId != task.PerformerUserProfileId)
            {
                var performerUpa =
                    await _uow.UserProfileAgregatesRepository
                        .GetQuery(x => x.UserProfileId == task.PerformerUserProfileId.Value, null)
                        .FirstOrDefaultAsync();
                notifications = notifications.Append(new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskCommentCreated,
                    ToUserProfileId = task.PerformerUserProfileId.Value,
                    ToUserProfileFullName = performerUpa?.FullName,
                    Date = now,
                    Data = JsonConvert.SerializeObject(
                        new BaseMessage
                        {
                            Id = task.Id,
                            Text = $"В задаче появился новый комментарий"
                        })
                });
            }

            _notificationService.Notify(notifications);
        }
    }
}