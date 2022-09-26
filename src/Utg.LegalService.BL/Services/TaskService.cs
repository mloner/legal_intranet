using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Utg.Common.Packages.Domain;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.Domain.Models.Enum;
using Utg.Common.Packages.FileStorage;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common;
using Utg.LegalService.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Request;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using TaskStatus = Utg.LegalService.Common.Models.Client.Enum.TaskStatus;

namespace Utg.LegalService.BL.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository taskRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IMapper mapper;
        private readonly IUsersProxyClient usersProxyClient;
        
        public TaskService(
            ITaskRepository taskRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IUsersProxyClient usersProxyClient)
        {
            this.taskRepository = taskRepository;
            this.fileStorageService = fileStorageService;
            this.mapper = mapper;
            this.usersProxyClient = usersProxyClient;
        }

        public async Task<PagedResult<TaskModel>> GetAll(TaskRequest request, AuthInfo authInfo)
        {
            var query = taskRepository.Get();

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
        
        private IQueryable<TaskModel> Filter(IQueryable<TaskModel> query, TaskRequest request)
        {
            if (request.Statuses != null && request.Statuses.Any())
            {
                query = query.Where(x => request.Statuses.Contains((int)x.Status));
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
                           || EF.Functions.ToTsVector(Const.PgFtsConfig, x.Description).Matches(EF.Functions.ToTsQuery(Const.PgFtsConfig, ftsQuery)))
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
            }

            return result;
        }

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
        {
            return new int[] { (int)Role.LegalHead, (int)Role.LegalInitiator}
                .Intersect(authInfo.Roles)
                .Any() &&
                   model.Status == TaskStatus.Draft;
        }
        
        private static bool CanDelete(TaskModel model, AuthInfo authInfo)
        {
            return authInfo.Roles.Contains((int)Role.LegalInitiator) &&
                   model.Status == TaskStatus.Draft;
        }
        
        private static bool CanMakeReport(TaskModel model, AuthInfo authInfo)
        {
            return authInfo.Roles.Contains((int)Role.LegalHead);
        }

        public async Task<TaskModel> GetById(int id, AuthInfo authInfo = null)
        {
            var task = await taskRepository.GetById(id);
            task.AccessRights = GetAccessRights(task, authInfo);
            return task;
        }
        
        public async Task<TaskModel> CreateTask(TaskCreateRequest request, AuthInfo authInfo)
        {
            var attachments = Enumerable.Empty<TaskAttachmentModel>();
            try
            {
                var inputModel = mapper.Map<TaskModel>(request);
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
            var performerUserProfile =
                (await usersProxyClient.GetByIdsAsync(new[] { model.PerformerUserProfileId.ToString() })).FirstOrDefault();
            if (performerUserProfile != null)
            {
                model.PerformerFullName = performerUserProfile.FullName;
            }
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

            await taskRepository.CreateAttachments(taskId, customAttachments);
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
                    await taskRepository.RemoveAttachments(taskId, request.RemovedAttachmentIds);
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

        public async Task DeleteTask(int id)
        {
            var task = await GetById(id);
            
            if (task.Attachments?.Any() == true)
            {
                await taskRepository.RemoveAttachments(task.Id, task.Attachments.Select(x => x.Id));
                foreach (var attachment in task.Attachments)
                {
                    await this.fileStorageService.DeleteFile(attachment.FileId);
                }
            }

            await taskRepository.DeleteTask(task.Id);
        }
    }
}