using System.Linq;
using System.Threading.Tasks;
using Utg.LegalService.Common.Models.Domain;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskCommentRepository
    {
        Task<TaskComment> CreateComment(TaskComment taskComment);
        IQueryable<TaskComment> Get();
    }
}