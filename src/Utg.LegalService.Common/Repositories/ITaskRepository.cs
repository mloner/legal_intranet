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
        IQueryable<Models.Domain.Task> Get();
        Task<TaskModel> GetById(int id);
        Task<TaskModel> CreateTask(TaskModel inputModel);
        Task UpdateTask(TaskModel model);
        Task DeleteTask(int taskId);
        Task UpdateTaskMoveToInWork(TaskModel newTask);
        Task UpdateTaskMoveToUnderReview(TaskModel newTask);
        Task UpdateTaskMoveToDone(TaskModel newTask);
    }
}
