using System.Linq;
using System.Threading.Tasks;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.BL.Services
{
    public class TaskService : ITaskService
    {
        private ITaskRepository taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            this.taskRepository = taskRepository;
        }

        public async Task<PagedResult<TaskModel>> GetAll(TaskRequest request)
        {
            var query = taskRepository.Get();

            var count = query.Count();

            if (request.Skip.HasValue)
            {
                query = query.Skip(request.Skip.Value);
            }
            if (request.Take.HasValue)
            {
                query = query.Take(request.Take.Value);
            }

            var list = query.ToList();
            
            return new PagedResult<TaskModel>()
            {
                Result = list,
                Total = count
            };
        }
    }
}