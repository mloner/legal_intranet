using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utg.LegalService.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request.Tasks;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskRepository
    {
        IQueryable<TaskModel> Get();
        Task<TaskModel> GetById(int id);
        Task<TaskModel> CreateTask(TaskModel inputModel);
        Task<IEnumerable<TaskAttachmentModel>>  CreateAttachments(int taskId, IEnumerable<TaskAttachmentModel> attachments);
        Task RemoveAttachments(int taskId, IEnumerable<int> attachmentIds);
        Task UpdateTask(TaskModel model);
        Task DeleteTask(int taskId);
    }
}
