using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Newtonsoft.Json;
using Utg.Common.Packages.Domain.Helpers;
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
using Utg.LegalService.BL.Features.AccessRights.Get;
using Utg.LegalService.BL.Features.Task.GetPage;
using Utg.LegalService.BL.Features.TaskChangeHistory.Create;
using Utg.LegalService.Common.Models.Client.Enum;
using Role = Utg.Common.Packages.Domain.Enums.Role;

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
        private readonly IUserProfileAgregateRepository _userProfileAgregateRepository;
        private readonly IExcelReportBuilder excelReportBuilder;
        private readonly IDataProxyClient dataProxyClient;
        private readonly INotificationService _notificationService;
        private readonly IMediator _mediator;

        public TaskService(
            ITaskRepository taskRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IUsersProxyClient usersProxyClient,
            IExcelReportBuilder excelReportBuilder,
            IDataProxyClient dataProxyClient,
            ITaskAttachmentRepository taskAttachmentRepository,
            ITaskCommentService taskCommentService,
            IUserProfileAgregateRepository userProfileAgregateRepository,
            INotificationService notificationService, 
            IMediator mediator)
        {
            this.taskRepository = taskRepository;
            this.fileStorageService = fileStorageService;
            this._mapper = mapper;
            this.usersProxyClient = usersProxyClient;
            this.excelReportBuilder = excelReportBuilder;
            this.dataProxyClient = dataProxyClient;
            _taskAttachmentRepository = taskAttachmentRepository;
            _taskCommentService = taskCommentService;
            _userProfileAgregateRepository = userProfileAgregateRepository;
            _notificationService = notificationService;
            _mediator = mediator;
        }
        
        public async Task<TaskModel> GetById(int id, AuthInfo authInfo = null)
        {
            var task = await taskRepository.GetById(id);
            var getTaskAccRightsComResp = 
                await _mediator.Send(new GetTaskAccessRightsCommand()
                {
                    Task = task,
                    AuthInfo = authInfo
                });
            task.AccessRights = getTaskAccRightsComResp.Data;
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
                   attachmentAuthorUserProfile.Roles.Contains((Utg.Common.Packages.ServiceClientProxy.Proxy.Role)(int)Role.LegalPerformer);
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

                if (createdTaskEnriched.Status != TaskStatus.Draft)
                {
                    var addHistoryComResp = 
                        await _mediator.Send(new CreateTaskChangeHistoryCommand()
                        {
                            TaskId = createdTaskEnriched.Id,
                            HistoryAction = HistoryAction.Created,
                            UserProfileId = authInfo.UserProfileId,
                            TaskStatus = createdTaskEnriched.Status
                        });
                    if (!addHistoryComResp.Success)
                    {
                        throw new Exception(addHistoryComResp.Message);
                    }   
                }

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
                    await dataProxyClient.UserProfilesRoleAsync((Utg.Common.Packages.ServiceClientProxy.Proxy.Role)(int)Role.LegalHead);
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
                var updatedTask = new TaskModel
                {
                    Id = request.Id,
                    Status = request.Status ?? oldTask.Status,
                    Type = request.Type ?? oldTask.Type,
                    Description = !string.IsNullOrEmpty(request.Description) ? request.Description : oldTask.Description,
                    PerformerUserProfileId = request.PerformerUserProfileId ?? oldTask.PerformerUserProfileId,
                    DeadlineDateTime = request.DeadlineDateTime ?? oldTask.DeadlineDateTime,
                    LastChangeDateTime = DateTime.SpecifyKind(DateTimeOffset.UtcNow.DateTime,
                            DateTimeKind.Utc)
                };
                await taskRepository.UpdateTask(updatedTask);

                if (request.AddedAttachments?.Any() == true)
                {
                    attachments = await this.AddAttachments(taskId, request.AddedAttachments, authInfo);
                }

                if (request.RemovedAttachmentIds?.Any() == true)
                {
                    await _taskAttachmentRepository.RemoveAttachments(taskId, request.RemovedAttachmentIds);
                }
                
                var addHistoryComResp = 
                    await _mediator.Send(new CreateTaskChangeHistoryCommand()
                    {
                        TaskId = updatedTask.Id,
                        HistoryAction = oldTask.Status == TaskStatus.Draft 
                            ? HistoryAction.Created 
                            : HistoryAction.Changed,
                        UserProfileId = authInfo.UserProfileId,
                        TaskStatus = updatedTask.Status
                    });
                if (!addHistoryComResp.Success)
                {
                    throw new Exception(addHistoryComResp.Message);
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

        private async Task FillPerformersToChildTasks(TaskModel oldTask, TaskModel updatedTask)
        {
            if (oldTask.PerformerUserProfileId != updatedTask.PerformerUserProfileId
                && updatedTask.PerformerUserProfileId != null)
            {
                var childTasksWithoutPerformer = await taskRepository.Get()
                    .Where(x => x.ParentTaskId == updatedTask.Id &&
                                !x.PerformerUserProfileId.HasValue)
                    .ToListAsync();
                foreach (var childTask in childTasksWithoutPerformer)
                {
                    childTask.PerformerUserProfileId = updatedTask.PerformerUserProfileId;
                }

                await taskRepository.UpdateTaskRange(childTasksWithoutPerformer);
            }
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
            
            var addHistoryComResp = 
                await _mediator.Send(new CreateTaskChangeHistoryCommand()
                {
                    TaskId = changedTask.Id,
                    HistoryAction = HistoryAction.StatusChanged,
                    UserProfileId = authInfo.UserProfileId,
                    TaskStatus = changedTask.Status
                });
            if (!addHistoryComResp.Success)
            {
                throw new Exception(addHistoryComResp.Message);
            }

            await FillPerformersToChildTasks(oldTask, changedTask);
            await TasksMoveToInWorkEmitEvents(oldTask, changedTask);
            
            return changedTask;
        }

        private async Task<TaskModel> UpdateTaskMoveToInWorkCommon(TaskUpdateMoveToInWorkRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var performer = await _userProfileAgregateRepository
                .GetQuery(x => true, null)
                .FirstOrDefaultAsync(
                    userProfile => userProfile.UserProfileId == request.PerformerUserProfileId);
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.InWork,
                PerformerUserProfileId = request.PerformerUserProfileId,
                PerformerFullName = performer?.FullName,
                DeadlineDateTime = request.DeadlineDateTime.HasValue? DateTime.SpecifyKind(request.DeadlineDateTime.Value, DateTimeKind.Utc) : null,
                LastChangeDateTime = DateTime.SpecifyKind(DateTimeOffset.UtcNow.DateTime,
                            DateTimeKind.Utc)
            };
            await taskRepository.UpdateTaskMoveToInWork(newTask);
            var result = await GetById(taskId, authInfo);
            
            return result;
        }

        private async Task TasksMoveToInWorkEmitEvents(TaskModel oldTask,
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
            
            var addHistoryComResp = 
                await _mediator.Send(new CreateTaskChangeHistoryCommand()
                {
                    TaskId = changedTask.Id,
                    HistoryAction = HistoryAction.StatusChanged,
                    UserProfileId = authInfo.UserProfileId,
                    TaskStatus = changedTask.Status
                });
            if (!addHistoryComResp.Success)
            {
                throw new Exception(addHistoryComResp.Message);
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
                await dataProxyClient.UserProfilesRoleAsync((Utg.Common.Packages.ServiceClientProxy.Proxy.Role)(int)Role.LegalHead);
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

            var addHistoryComResp = 
                await _mediator.Send(new CreateTaskChangeHistoryCommand()
                {
                    TaskId = changedTask.Id,
                    HistoryAction = HistoryAction.StatusChanged,
                    UserProfileId = authInfo.UserProfileId,
                    TaskStatus = changedTask.Status
                });
            if (!addHistoryComResp.Success)
            {
                throw new Exception(addHistoryComResp.Message);
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

        public async Task<IEnumerable<UserProfileApiModel>> GetPerformerUserProfiles(AuthInfo authInfo)
        {
            var result = await dataProxyClient.UserProfilesRoleAsync((Utg.Common.Packages.ServiceClientProxy.Proxy.Role)(int)Role.LegalPerformer);
            if (!authInfo.Roles.Contains((int)Role.LegalHead))
                result = result.Where(userProfile => userProfile.Id == authInfo.UserProfileId);
            return result;
        }

        public async Task<UserProfileApiModel> GetUserProfileById(int id)        
        {
            var result = await usersProxyClient.GetByIdsAsync(new List<string> { id.ToString()});
            return result?.FirstOrDefault();
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

        public async Task<Stream> GetReport(GetTaskPageReportRequest request, AuthInfo authInfo,
            HttpContext httpContext)
        {
            TimeSpan offset = TimeSpan.Zero;
            if (httpContext.Request.Headers.TryGetValue("TimeZoneOffsetMinutes",
                    out var values))
            {
                if (int.TryParse(values.First(), out var value))
                {
                    offset = new TimeSpan(0, value, 0);
                }
            }

            var cmd = request.Adapt<GetTaskPageCommand>();
            cmd.AuthInfo = authInfo;
            var data = await _mediator.Send(cmd);

            var reportData = data.Data.Select((x, index) => new TaskReportDto()
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