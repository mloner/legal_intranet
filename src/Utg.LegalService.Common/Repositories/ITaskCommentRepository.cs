using System.Linq;
using System.Threading.Tasks;
using Utg.Common.EF.Repositories;
using Utg.LegalService.Common.Models.Domain;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskCommentRepository
        : IBaseRepositoryAdvanced<TaskComment>
    {
        Task<TaskComment> CreateComment(TaskComment taskComment);
        IQueryable<TaskComment> Get();
    }
}