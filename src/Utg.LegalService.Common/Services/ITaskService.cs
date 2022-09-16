using System.Threading.Tasks;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request;

namespace Utg.LegalService.Common.Services
{
    public interface ITaskService
    {
        Task<PagedResult<TaskModel>> GetAll(TaskRequest request);
    }
}