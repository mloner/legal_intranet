using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using Newtonsoft.Json;
using Utg.Common.Packages.Domain;
using Utg.Common.Packages.Domain.Helpers;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.Domain.Models.Notification;
using Utg.Common.Packages.Domain.Models.Push;
using Utg.Common.Packages.ExcelReportBuilder;
using Utg.Common.Packages.FileStorage;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Report.Dtos;
using Utg.LegalService.Common.Models.Report.Helpers;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using NotificationTaskType = Utg.Common.Packages.Domain.Enums.NotificationTaskType;
using TaskStatus = Utg.LegalService.Common.Models.Client.Enum.TaskStatus;
using Utg.Common.Packages.Domain.Enums;

namespace Utg.LegalService.BL.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository taskRepository;
        private readonly ITaskAttachmentRepository _taskAttachmentRepository;
        private readonly ITaskCommentService _taskCommentService;
        private readonly IFileStorageService fileStorageService;
        private readonly IMapper _mapper;
        private readonly IUsersProxyClient usersProxyClient;
        private readonly IAgregateRepository _agregateRepository;
        private readonly IExcelReportBuilder excelReportBuilder;
        private readonly IDataProxyClient dataProxyClient;
        private readonly INotificationService _notificationService;

        public TaskService(
            ITaskRepository taskRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IUsersProxyClient usersProxyClient,
            IExcelReportBuilder excelReportBuilder,
            IDataProxyClient dataProxyClient,
            ITaskAttachmentRepository taskAttachmentRepository,
            ITaskCommentService taskCommentService,
            IAgregateRepository agregateRepository,
            INotificationService notificationService)
        {
            this.taskRepository = taskRepository;
            this.fileStorageService = fileStorageService;
            this._mapper = mapper;
            this.usersProxyClient = usersProxyClient;
            this.excelReportBuilder = excelReportBuilder;
            this.dataProxyClient = dataProxyClient;
            _taskAttachmentRepository = taskAttachmentRepository;
            _taskCommentService = taskCommentService;
            _agregateRepository = agregateRepository;
            _notificationService = notificationService;
        }

        public async Task<PagedResult<TaskModel>> GetAll(GetTaskPageRequest request, AuthInfo authInfo)
        {
            var query = taskRepository.Get()
                .OrderByDescending(task => task.CreationDateTime)
                .ProjectTo<TaskModel>(_mapper.ConfigurationProvider);

            query = FilterByRoles(query, request, authInfo);
            query = Filter(query, request);
            query = Search(query, request);

            var count = query.Count();
            query = SkipAndTake(query, request);
            
            var list = query.AsEnumerable();
            list = GetTasksAccessRights(list, authInfo);

            return new PagedResult<TaskModel>()
            {
                Result = list,
                Total = count
            };
        }

        private IQueryable<TaskModel> FilterByRoles(IQueryable<TaskModel> query, GetTaskPageRequest request, AuthInfo authInfo)
        {
            var predicate = PredicateBuilder.New<TaskModel>(true);

            if (authInfo.Roles.Contains((int)Role.IntranetUser))
            {
                predicate = PredicateBuilder.New<TaskModel>(
                    model => model.AuthorUserProfileId == authInfo.UserProfileId);
            }

            if (authInfo.Roles.Contains((int)Role.LegalPerformer))
            {
                predicate = predicate.Or(
                    model => model.PerformerUserProfileId == authInfo.UserProfileId);
            }

            query = query.Where(predicate).AsQueryable();
            
            query = query.Where(x => !(x.Status == TaskStatus.Draft && x.AuthorUserProfileId != authInfo.UserProfileId));

            return query;
        }

        private IQueryable<TaskModel> Filter(IQueryable<TaskModel> query, GetTaskPageRequest request)
        {
            if (request.Statuses != null && request.Statuses.Any())
            {
                query = query.Where(x => request.Statuses.Contains((int)x.Status));
            }
            else
            {
                query = query.Where(x => x.Status != TaskStatus.Done);
            }

            if (request.AuthorUserProfileIds != null && request.AuthorUserProfileIds.Any())
            {
                query = query.Where(x => request.AuthorUserProfileIds.Contains(x.AuthorUserProfileId));
            }

            return query;
        }

        private IQueryable<TaskModel> Search(IQueryable<TaskModel> query, GetTaskPageRequest request)
        {
            if (!string.IsNullOrEmpty(request.Search))
            {
                var ftsQuery = Util.GetFullTextSearchQuery(request.Search);
                var ilikeQuery = $"%{request.Search}%";

                query = query.Where(x
                        => EF.Functions.ILike(x.AuthorFullName, ilikeQuery) 
                           || EF.Functions.ToTsVector(Const.PgFtsConfig, x.AuthorFullName)
                               .Matches(EF.Functions.PlainToTsQuery(Const.PgFtsConfig, ftsQuery))
                           
                           || EF.Functions.ILike(x.Description, ilikeQuery) 
                           || EF.Functions.ToTsVector(Const.PgFtsConfig, x.Description)
                               .Matches(EF.Functions.PlainToTsQuery(Const.PgFtsConfig, ftsQuery))
                           );
            }

            return query;
        }

        private IQueryable<TaskModel> SkipAndTake(IQueryable<TaskModel> query, GetTaskPageRequest request)
        {
            if (request.Skip.HasValue)
            {
                query = query.Skip(request.Skip.Value);
            }
            if (request.Take.HasValue)
            {
                query = query.Take(request.Take.Value);
            }

            return query;
        }
        private IEnumerable<TaskModel> GetTasksAccessRights(IEnumerable<TaskModel> models, AuthInfo authInfo)
        {

            models = models.Select(x =>
            {
                x.AccessRights = GetTaskAccessRights(x, authInfo);
                return x;
            });

            return models;
        }
        private static TaskAccessRights GetTaskAccessRights(TaskModel model, AuthInfo authInfo)
        {
            var result = new TaskAccessRights();
            if (authInfo != null)
            {
                result.IsPerformerAvailable = IsPerformerAvailable(authInfo);
                result.CanShowDetails = CanShowDetails(authInfo);
                result.CanEdit = CanEdit(model, authInfo);
                result.CanDelete = CanDelete(model, authInfo);
                result.CanMakeReport = CanMakeReport(model, authInfo);
                result.CanPerform = CanPerform(model, authInfo);
                result.CanReview = CanReview(model, authInfo);
            }

            return result;
        }

        private static bool CanReview(TaskModel model, AuthInfo authInfo)
         => authInfo.Roles.Contains((int)Role.LegalHead) &&
                    model.Status == TaskStatus.UnderReview;

        private static bool IsPerformerAvailable(AuthInfo authInfo)
        {
            return authInfo.Roles.Contains((int)Role.LegalHead);
        }

        private static bool CanShowDetails(AuthInfo authInfo)
        {
            return new int[] { (int)Role.LegalHead, (int)Role.IntranetUser, (int)Role.LegalPerformer }
                    .Intersect(authInfo.Roles)
                    .Any();
        }
        private static bool CanEdit(TaskModel model, AuthInfo authInfo)
           => authInfo.Roles.Contains((int)Role.IntranetUser)
                && model.Status == TaskStatus.Draft ||
            authInfo.Roles.Contains((int)Role.LegalHead)
                && model.Status == TaskStatus.New;


        private static bool CanDelete(TaskModel model, AuthInfo authInfo)
        {
            return authInfo.Roles.Contains((int)Role.IntranetUser) &&
                   model.Status == TaskStatus.Draft;
        }

        private static bool CanMakeReport(TaskModel model, AuthInfo authInfo)
        {
            return authInfo.Roles.Contains((int)Role.LegalHead);
        }

        private static bool CanPerform(TaskModel model, AuthInfo authInfo)
                => authInfo.Roles.Contains((int)Role.LegalPerformer) &&
                    model.Status == TaskStatus.InWork &&
                    model.PerformerUserProfileId == authInfo.UserProfileId;


        public async Task<TaskModel> GetById(int id, AuthInfo authInfo = null)
        {
            var task = await taskRepository.GetById(id);
            task.AccessRights = GetTaskAccessRights(task, authInfo);
            task.TaskComments = await _taskCommentService.GetByTaskId(task.Id);
            var attachmentsTasks = task.Attachments.Select(async attachment => 
            {
                attachment.AccessRights = await GetAttachmentAccessRights(task, attachment, authInfo);
                return attachment;
            }).ToList();
            task.Attachments = await Task.WhenAll(attachmentsTasks);
            return task;
        }

        private async Task<AttachmentAccessRights> GetAttachmentAccessRights(TaskModel task,
            TaskAttachmentModel attachment, AuthInfo authInfo)
        {
            var result = new AttachmentAccessRights();
            if (authInfo != null)
            {
                var attachmentAuthorUserProfile =
                    (await usersProxyClient.GetByIdsAsync(
                        new []{(attachment.UserProfileId == null ? 
                            0 : attachment.UserProfileId.Value).ToString()})).FirstOrDefault();
                result.CanDelete = CanDeleteAttachment(task, attachment, authInfo, attachmentAuthorUserProfile);
            }

            return result;
        }

        private bool CanDeleteAttachment(TaskModel task, 
            TaskAttachmentModel attachment, AuthInfo authInfo,
            UserProfileApiModel attachmentAuthorUserProfile)
        {
            if (!attachment.UserProfileId.HasValue || attachmentAuthorUserProfile == null)
            {
                return true;
            }

            var isFileOwn = attachment.UserProfileId.Value == authInfo.UserProfileId;
            return isFileOwn ||
                   authInfo.Roles.Contains((int)Role.LegalHead) &&
                   attachmentAuthorUserProfile.Roles.Contains((int)Role.LegalPerformer);
        }

        public async Task<TaskModel> CreateTask(TaskCreateRequest request, AuthInfo authInfo)
        {
            var attachments = Enumerable.Empty<TaskAttachmentModel>();
            try
            {
                var inputModel = _mapper.Map<TaskModel>(request);
                await FillCreateTaskModel(inputModel, authInfo);

                var createdTask = await taskRepository.CreateTask(inputModel);

                if (request.Attachments?.Any() == true)
                {
                    attachments = await AddAttachments(createdTask.Id, request.Attachments, authInfo);
                }

                var createdTaskEnriched = await GetById(createdTask.Id, authInfo);

                await CreateTaskEmitEvents(createdTaskEnriched);
 
                return createdTaskEnriched;
            }
            catch (Exception e)
            {
                await DeleteAttachmentFiles(attachments);
                throw;
            }
        }

        private async Task CreateTaskEmitEvents(TaskModel taskModel)
        {
            var now = DateTime.UtcNow;
            var notifications = Enumerable.Empty<NotificationModel>();
            if (taskModel.Status == TaskStatus.New)
            {
                var legalHeadUserProfiles =
                    await dataProxyClient.UserProfilesRoleAsync((int)Role.LegalHead);
                foreach (var legalHeadUserProfile in legalHeadUserProfiles)
                {
                    notifications = notifications.Append(new NotificationModel
                    {
                        NotificationType = NotificationTaskType.LegalTaskCreated,
                        ToUserProfileId = legalHeadUserProfile.Id,
                        ToUserProfileFullName = legalHeadUserProfile.FullName,
                        Date = now,
                        Data = JsonConvert.SerializeObject(
                            new BaseMessage
                            {
                                Id = taskModel.Id,
                                Text = $"Создана задача"
                            })
                    });
                }
            }
            
            _notificationService.Notify(notifications);
        }

        private async Task FillCreateTaskModel(TaskModel model, AuthInfo authInfo)
        {
            model.AuthorUserProfileId = authInfo.UserProfileId;
            model.AuthorFullName = authInfo.FullName;
            var now = DateTime.UtcNow;
            model.CreationDateTime = now;
            model.LastChangeDateTime = now;
        }

        private async Task DeleteAttachmentFiles(IEnumerable<TaskAttachmentModel> attachments)
        {
            if (attachments?.Any() == true)
            {
                foreach (var attachment in attachments)
                {
                    await this.fileStorageService.DeleteFile(attachment.FileId);
                }
            }
        }

        private async Task<IEnumerable<TaskAttachmentModel>> AddAttachments(int taskId,
            IEnumerable<IFormFile> attachments, AuthInfo authInfo)
        {
            var customAttachments = new List<TaskAttachmentModel>();

            foreach (var attachment in attachments)
            {
                var attachmentFileId =
                    await this.fileStorageService.SaveFile(attachment.OpenReadStream(),
                        attachment.ContentType);

                customAttachments.Add(
                    new TaskAttachmentModel()
                    {
                        TaskId = taskId,
                        FileId = attachmentFileId,
                        FileName = attachment.FileName,
                        FileSizeInBytes = attachment.Length,
                        UserProfileId = authInfo.UserProfileId
                    });
            }

            await _taskAttachmentRepository.CreateAttachments(taskId, customAttachments);
            return customAttachments;
        }

        public async Task<TaskModel> UpdateTask(TaskUpdateRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var oldTask = await GetById(taskId, authInfo);
            var attachments = Enumerable.Empty<TaskAttachmentModel>();

            try
            {
                var newTask = new TaskModel
                {
                    Id = request.Id,
                    Status = request.Status ?? oldTask.Status,
                    Type = request.Type ?? oldTask.Type,
                    Description = !string.IsNullOrEmpty(request.Description) ? request.Description : oldTask.Description,
                    PerformerUserProfileId = request.PerformerUserProfileId ?? oldTask.PerformerUserProfileId,
                    DeadlineDateTime = request.DeadlineDateTime ?? oldTask.DeadlineDateTime,
                    LastChangeDateTime = DateTime.SpecifyKind(DateTimeOffset.UtcNow.DateTime,
                            DateTimeKind.Utc)
                ,
                };
                await taskRepository.UpdateTask(newTask);

                if (request.AddedAttachments?.Any() == true)
                {
                    attachments = await this.AddAttachments(taskId, request.AddedAttachments, authInfo);
                }

                if (request.RemovedAttachmentIds?.Any() == true)
                {
                    await _taskAttachmentRepository.RemoveAttachments(taskId, request.RemovedAttachmentIds);
                }
            }
            catch (Exception e)
            {
                await DeleteAttachmentFiles(attachments);
                throw;
            }

            if (request.RemovedAttachmentIds?.Any() == true)
            {
                var filesToRemove = oldTask.Attachments
                    .Where(attachment => request.RemovedAttachmentIds.Contains(attachment.Id))
                    .Select(attachment => attachment.FileId);

                foreach (var fileId in filesToRemove)
                {
                    await this.fileStorageService.DeleteFile(fileId);
                }
            }
            var result = await GetById(taskId, authInfo);
            return result;
        }

        public async Task<TaskModel> UpdateTaskMoveToInWork(TaskUpdateMoveToInWorkRequest request, AuthInfo authInfo)
        {
            TaskModel changedTask;
            var oldTask = await GetById(request.Id, authInfo);

            switch (oldTask.Status)
            {
                case TaskStatus.UnderReview:
                default:
                    changedTask = await UpdateTaskMoveToInWorkCommon(request, authInfo);
                    break;
            }

            await UpdateTaskMoveToInWorkEmitEvents(oldTask, changedTask);
            
            return changedTask;
        }

        private async Task<TaskModel> UpdateTaskMoveToInWorkCommon(TaskUpdateMoveToInWorkRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var performer = await _agregateRepository
                .GetQuery(x => true, null)
                .FirstOrDefaultAsync(
                    userProfile => userProfile.UserProfileId == request.PerformerUserProfileId);
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.InWork,
                PerformerUserProfileId = request.PerformerUserProfileId,
                PerformerFullName = performer?.FullName,
                DeadlineDateTime = DateTime.SpecifyKind(request.DeadlineDateTime, DateTimeKind.Utc),
                LastChangeDateTime = DateTime.SpecifyKind(DateTimeOffset.UtcNow.DateTime,
                            DateTimeKind.Utc)
            };
            await taskRepository.UpdateTaskMoveToInWork(newTask);
            var result = await GetById(taskId, authInfo);
            return result;
        }

        private async Task UpdateTaskMoveToInWorkEmitEvents(TaskModel oldTask,
            TaskModel changedTask)
        {
            var now = DateTime.UtcNow;
            var notifications = Enumerable.Empty<NotificationModel>();
            if (oldTask.Status == TaskStatus.New)
            {
                notifications = notifications.Append(new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskStatusChanged,
                    ToUserProfileId = changedTask.AuthorUserProfileId,
                    ToUserProfileFullName = changedTask.AuthorFullName,
                    Date = now,
                    Data = JsonConvert.SerializeObject(
                        new BaseMessage
                        {
                            Id = changedTask.Id,
                            Text = $"Статус задачи изменён на \"{changedTask.Status.GetDisplayName()}\""
                        })
                });
            }
            notifications = notifications.Append(new NotificationModel
            {
                NotificationType = NotificationTaskType.LegalTaskStatusChanged,
                ToUserProfileId = changedTask.PerformerUserProfileId.Value,
                ToUserProfileFullName = changedTask.PerformerFullName,
                Date = now,
                Data = JsonConvert.SerializeObject(
                    new BaseMessage
                    {
                        Id = changedTask.Id,
                        Text = $"Статус задачи изменён на \"{changedTask.Status.GetDisplayName()}\""
                    })
            });
            
            _notificationService.Notify(notifications);
        }
        
        public async Task<TaskModel> UpdateTaskMoveToUnderReview(TaskUpdateMoveToUnderReviewRequest request, AuthInfo authInfo)
        {
            TaskModel changedTask;
            var oldTask = await GetById(request.Id, authInfo);

            switch (oldTask.Status)
            {
                default:
                    changedTask = await UpdateTaskMoveToUnderReviewCommon(request, authInfo);
                    break;
            }
            
            await UpdateTaskMoveToUnderReviewEmitEvents(oldTask, changedTask);

            return changedTask;
        }
        
        private async Task UpdateTaskMoveToUnderReviewEmitEvents(TaskModel oldTask,
            TaskModel changedTask)
        {
            var now = DateTime.UtcNow;
            var notifications = Enumerable.Empty<NotificationModel>();
            var legalHeadUserProfiles = 
                await dataProxyClient.UserProfilesRoleAsync((int)Role.LegalHead);
            foreach (var legalHeadUserProfile in legalHeadUserProfiles)
            {
                notifications = notifications.Append(new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskStatusChanged,
                    ToUserProfileId = legalHeadUserProfile.Id,
                    ToUserProfileFullName = legalHeadUserProfile.FullName,
                    Date = now,
                    Data = JsonConvert.SerializeObject(
                        new BaseMessage
                        {
                            Id = changedTask.Id,
                            Text = $"Статус задачи изменён на \"{changedTask.Status.GetDisplayName()}\""
                        })
                });
            }
            
            _notificationService.Notify(notifications);
        }

        private async Task<TaskModel> UpdateTaskMoveToUnderReviewCommon(TaskUpdateMoveToUnderReviewRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.UnderReview,
                LastChangeDateTime = DateTime.SpecifyKind(DateTimeOffset.UtcNow.DateTime,
                            DateTimeKind.Utc)
            };
            await taskRepository.UpdateTaskMoveToUnderReview(newTask);
            var result = await GetById(taskId, authInfo);
            return result;
        }

        public async Task<TaskModel> UpdateTaskMoveToDone(TaskUpdateMoveToDoneRequest request, AuthInfo authInfo)
        {
            TaskModel changedTask;
            var oldTask = await GetById(request.Id, authInfo);
            switch (oldTask.Status)
            {
                case TaskStatus.UnderReview:
                default:
                    changedTask = await UpdateTaskMoveToDoneCommon(request, authInfo);
                    break;
            }

            await UpdateTaskMoveToDoneEmitEvents(oldTask, changedTask);
            
            return changedTask;
        }
        
        private async Task UpdateTaskMoveToDoneEmitEvents(TaskModel oldTask,
            TaskModel changedTask)
        {
            var now = DateTime.UtcNow;
            var notifications = Enumerable.Empty<NotificationModel>();
            if (oldTask.Status == TaskStatus.UnderReview)
            {
                notifications = notifications.Append(new NotificationModel
                {
                    NotificationType = NotificationTaskType.LegalTaskStatusChanged,
                    ToUserProfileId = changedTask.AuthorUserProfileId,
                    ToUserProfileFullName = changedTask.AuthorFullName,
                    Date = now,
                    Data = JsonConvert.SerializeObject(
                        new BaseMessage
                        {
                            Id = changedTask.Id,
                            Text = $"Статус задачи изменён на \"{changedTask.Status.GetDisplayName()}\""
                        })
                });
            }
            
            _notificationService.Notify(notifications);
        }

        private async Task<TaskModel> UpdateTaskMoveToDoneCommon(TaskUpdateMoveToDoneRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.Done,
                LastChangeDateTime = DateTime.SpecifyKind(DateTimeOffset.UtcNow.DateTime,
                            DateTimeKind.Utc)
            };
            await taskRepository.UpdateTaskMoveToDone(newTask);
            var result = await GetById(taskId, authInfo);
            return result;
        }

        public async Task<IEnumerable<UserProfileApiModel>> GetPerformerUserProfiles()
        {
            var result = dataProxyClient.UserProfilesRoleAsync((int)Role.LegalPerformer);
            return result.Result;
        }

        public async Task UploadFile(TaskUploadFileRequest request, AuthInfo authInfo)
        {
            var attachments = Enumerable.Empty<TaskAttachmentModel>();
            try
            {
                if (request.Attachments?.Any() == true)
                {
                    attachments = await this.AddAttachments(request.TaskId, request.Attachments, authInfo);
                }
            }
            catch (Exception e)
            {
                await DeleteAttachmentFiles(attachments);
                throw;
            }
        }

        public async Task<TaskAttachmentModel> DownloadFile(int attachmentId)
        {
            var attachment = await _taskAttachmentRepository.Get()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == attachmentId);
            var attachmentModel = _mapper.Map<TaskAttachmentModel>(attachment);
            var file = await fileStorageService.GetFile(attachment.FileId);
            attachmentModel.Bytes = file;
            return attachmentModel;
        }

        public async Task DeleteFile(int attachmentId)
        {
            var attachment = await _taskAttachmentRepository.Get()
                .FirstOrDefaultAsync(x => x.Id == attachmentId);
            await _taskAttachmentRepository.Delete(attachment);
            await this.fileStorageService.DeleteFile(attachment.FileId);
        }

        public async Task DeleteTask(int id)
        {
            var task = await GetById(id);

            if (task.Attachments?.Any() == true)
            {
                await _taskAttachmentRepository.RemoveAttachments(task.Id, task.Attachments.Select(x => x.Id));
                foreach (var attachment in task.Attachments)
                {
                    await this.fileStorageService.DeleteFile(attachment.FileId);
                }
            }

            await taskRepository.DeleteTask(task.Id);
        }

        public async Task<IEnumerable<UserProfileApiModel>> GetAuthorUserProfiles()
        {
            var authorUserProfileIds = await taskRepository.Get()
                .Select(x => x.AuthorUserProfileId)
                .Distinct()
                .ToListAsync();
            var userProfiles = await usersProxyClient.GetByIdsAsync(authorUserProfileIds.Select(x => x.ToString()));
            return userProfiles;
        }

        public async Task<Stream> GetReport(GetTaskPageReportRequest request, AuthInfo authInfo)
        {
            var offset = request.TimeZoneOffsetMinutes.HasValue 
                ? new TimeSpan(0, request.TimeZoneOffsetMinutes.Value, 0) 
                : TimeSpan.Zero;

            var data = await GetAll(new GetTaskPageRequest()
            {
                Statuses = request.Statuses,
                AuthorUserProfileIds = request.AuthorUserProfileIds
            }, authInfo);

            var reportData = data.Result.Select((x, index) => new TaskReportDto()
            {
                RowNumber = x.Id,
                ParentTaskId = x.ParentTaskId,
                CreationDate = x.CreationDateTime.Add(offset),
                AuthorFullName = x.AuthorFullName,
                PerformerFullName = x.PerformerFullName,
                TaskType = x.TypeName,
                Status = x.StatusName,
                Deadline = x.DeadlineDateTime?.Add(offset),
                LastChangeDateTime = x.LastChangeDateTime.Add(offset)
            });

            var reportStream = excelReportBuilder.BuildNewReport(builder =>
            {
                if (!reportData.Any())
                {
                    builder
                        .AddSheetFromTemplateCurrentDomain(TaskReportConstants.TemplatePath, "Лист1");
                }
                else
                {
                    builder
                        .AddSheetFromTemplateCurrentDomain(TaskReportConstants.TemplatePath, "Лист1")
                        .PrintData(reportData, TaskReportConstants.DataStartRow)
                        .CustomAction(worksheet =>
                        {
                            worksheet.Columns("E:J").AdjustToContents();
                        });
                }
            });

            return reportStream;
        }
    }
}