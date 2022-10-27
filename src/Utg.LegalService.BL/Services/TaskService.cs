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
using Utg.Common.Packages.Domain;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.Domain.Models.Enum;
using Utg.Common.Packages.ExcelReportBuilder;
using Utg.Common.Packages.FileStorage;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Report.Dtos;
using Utg.LegalService.Common.Models.Report.Helpers;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using TaskStatus = Utg.LegalService.Common.Models.Client.Enum.TaskStatus;

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

        public TaskService(
            ITaskRepository taskRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IUsersProxyClient usersProxyClient,
            IExcelReportBuilder excelReportBuilder,
            IDataProxyClient dataProxyClient,
            ITaskAttachmentRepository taskAttachmentRepository,
            ITaskCommentService taskCommentService,
            IAgregateRepository agregateRepository)
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
        }

        public async Task<PagedResult<TaskModel>> GetAll(TaskRequest request, AuthInfo authInfo)
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
            list = FillAccessRights(list, authInfo);

            return new PagedResult<TaskModel>()
            {
                Result = list,
                Total = count
            };
        }


        private IQueryable<TaskModel> FilterByRoles(IQueryable<TaskModel> query, TaskRequest request, AuthInfo authInfo)
        {
            var predicate = PredicateBuilder.New<TaskModel>(true);

            if (authInfo.Roles.Contains((int)Role.LegalInitiator))
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

        private IQueryable<TaskModel> Filter(IQueryable<TaskModel> query, TaskRequest request)
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

        private IQueryable<TaskModel> Search(IQueryable<TaskModel> query, TaskRequest request)
        {
            if (!string.IsNullOrEmpty(request.Search))
            {
                var ftsQuery = Util.GetFullTextSearchQuery(request.Search);
                var ilikeQuery = $"%{request.Search}%";

                query = query.Where(x
                        => EF.Functions.ILike(x.Description, ilikeQuery)
                           || EF.Functions.ToTsVector(Const.PgFtsConfig, x.Description)
                               .Matches(EF.Functions.PlainToTsQuery(Const.PgFtsConfig, ftsQuery)))
                    ;
            }

            return query;
        }

        private IQueryable<TaskModel> SkipAndTake(IQueryable<TaskModel> query, TaskRequest request)
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
        private IEnumerable<TaskModel> FillAccessRights(IEnumerable<TaskModel> models, AuthInfo authInfo)
        {

            models = models.Select(x =>
            {
                x.AccessRights = GetAccessRights(x, authInfo);

                return x;
            });

            return models;
        }
        private static TaskAccessRights GetAccessRights(TaskModel model, AuthInfo authInfo)
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
            return new int[] { (int)Role.LegalHead, (int)Role.LegalInitiator, (int)Role.LegalPerformer }
                    .Intersect(authInfo.Roles)
                    .Any();
        }
        private static bool CanEdit(TaskModel model, AuthInfo authInfo)
           => authInfo.Roles.Contains((int)Role.LegalInitiator)
                && model.Status == TaskStatus.Draft ||
            authInfo.Roles.Contains((int)Role.LegalHead)
                && model.Status == TaskStatus.New;


        private static bool CanDelete(TaskModel model, AuthInfo authInfo)
        {
            return authInfo.Roles.Contains((int)Role.LegalInitiator) &&
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
            task.AccessRights = GetAccessRights(task, authInfo);
            task.TaskComments = await _taskCommentService.GetByTaskId(task.Id);
            return task;
        }

        public async Task<TaskModel> CreateTask(TaskCreateRequest request, AuthInfo authInfo)
        {
            var attachments = Enumerable.Empty<TaskAttachmentModel>();
            try
            {
                var inputModel = _mapper.Map<TaskModel>(request);
                await FillCreateTaskModel(inputModel, authInfo);

                var task = await taskRepository.CreateTask(inputModel);

                if (request.Attachments?.Any() == true)
                {
                    attachments = await AddAttachments(task.Id, request.Attachments);
                }

                var result = await GetById(task.Id, authInfo);
                return result;
            }
            catch (Exception e)
            {
                await DeleteAttachmentFiles(attachments);
                throw;
            }
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

        private async Task<IEnumerable<TaskAttachmentModel>> AddAttachments(int taskId, IEnumerable<IFormFile> attachments)
        {
            var customAttachments = new List<TaskAttachmentModel>();

            foreach (var attachment in attachments)
            {
                var attachmentFileId = await this.fileStorageService.SaveFile(attachment.OpenReadStream(), attachment.ContentType);

                customAttachments.Add(
                    new TaskAttachmentModel()
                    {
                        TaskId = taskId,
                        FileId = attachmentFileId,
                        FileName = attachment.FileName,
                        FileSizeInBytes = attachment.Length
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
                    LastChangeDateTime = DateTimeOffset.UtcNow.DateTime,
                };
                await taskRepository.UpdateTask(newTask);

                if (request.AddedAttachments?.Any() == true)
                {
                    attachments = await this.AddAttachments(taskId, request.AddedAttachments);
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
            TaskModel result;
            var task = await GetById(request.Id, authInfo);

            switch (task.Status)
            {
                case TaskStatus.UnderReview:
                default:
                    result = await UpdateTaskMoveToInWorkCommon(request, authInfo);
                    break;
            }

            return result;
        }

        private async Task<TaskModel> UpdateTaskMoveToInWorkCommon(TaskUpdateMoveToInWorkRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var performer = await _agregateRepository.GetUserProfiles().FirstOrDefaultAsync(userProfile => userProfile.UserProfileId == request.PerformerUserProfileId);
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.InWork,
                PerformerUserProfileId = request.PerformerUserProfileId,
                PerformerFullName = performer?.FullName,
                DeadlineDateTime = request.DeadlineDateTime,
                LastChangeDateTime = DateTimeOffset.UtcNow.DateTime,
            };
            await taskRepository.UpdateTaskMoveToInWork(newTask);
            var result = await GetById(taskId, authInfo);
            return result;
        }

        public async Task<TaskModel> UpdateTaskMoveToUnderReview(TaskUpdateMoveToUnderReviewRequest request, AuthInfo authInfo)
        {
            TaskModel result;
            var task = await GetById(request.Id, authInfo);

            switch (task.Status)
            {
                default:
                    result = await UpdateTaskMoveToUnderReviewCommon(request, authInfo);
                    break;
            }

            return result;
        }

        private async Task<TaskModel> UpdateTaskMoveToUnderReviewCommon(TaskUpdateMoveToUnderReviewRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.UnderReview,
                LastChangeDateTime = DateTimeOffset.UtcNow.DateTime,
            };
            await taskRepository.UpdateTaskMoveToUnderReview(newTask);
            var result = await GetById(taskId, authInfo);
            return result;
        }

        public async Task<TaskModel> UpdateTaskMoveToDone(TaskUpdateMoveToDoneRequest request, AuthInfo authInfo)
        {
            TaskModel result;
            var task = await GetById(request.Id, authInfo);
            switch (task.Status)
            {
                case TaskStatus.UnderReview:
                default:
                    result = await UpdateTaskMoveToDoneCommon(request, authInfo);
                    break;
            }


            return result;
        }

        private async Task<TaskModel> UpdateTaskMoveToDoneCommon(TaskUpdateMoveToDoneRequest request, AuthInfo authInfo)
        {
            var taskId = request.Id;
            var newTask = new TaskModel
            {
                Id = taskId,
                Status = TaskStatus.Done,
                LastChangeDateTime = DateTimeOffset.UtcNow.DateTime,
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
                    attachments = await this.AddAttachments(request.TaskId, request.Attachments);
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

        public async Task<Stream> GetReport(TaskReportRequest request, AuthInfo authInfo)
        {
            var data = await GetAll(new TaskRequest()
            {
                Statuses = request.Statuses,
                AuthorUserProfileIds = request.AuthorUserProfileIds
            }, authInfo);

            var reportData = data.Result.Select((x, index) => new TaskReportDto()
            {
                RowNumber = index + 1,
                CreationDate = x.CreationDateTime,
                AuthorFullName = x.AuthorFullName,
                PerformerFullName = x.PerformerFullName,
                TaskType = x.TypeName,
                Status = x.StatusName,
                Deadline = x.DeadlineDateTime,
                LastChangeDateTime = x.LastChangeDateTime
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
                        .PrintData(reportData, TaskReportConstants.DataStartRow);
                }
            });

            return reportStream;
        }
    }
}