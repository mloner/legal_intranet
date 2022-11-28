using System.Linq;
using System.Threading.Tasks;
using Utg.Common.EF.Repositories;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskRepository : IBaseRepositoryAdvanced<Models.Domain.Task>
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
