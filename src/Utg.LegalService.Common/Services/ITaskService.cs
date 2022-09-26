using System.Threading.Tasks;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request;
using Utg.LegalService.Common.Models.Request.Tasks;

namespace Utg.LegalService.Common.Services
{
    public interface ITaskService
    {
        Task<PagedResult<TaskModel>> GetAll(TaskRequest request, AuthInfo authInfo);
        Task<TaskModel> GetById(int id, AuthInfo authInfo);
        Task<TaskModel> CreateTask(TaskCreateRequest request, AuthInfo authInfo);
        Task<TaskModel> UpdateTask(TaskUpdateRequest request, AuthInfo authInfo);
        Task DeleteTask(int id);
    }
}