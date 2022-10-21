using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal.Repositories
{
    public class TaskCommentRepository : ITaskCommentRepository
    {
        private readonly UtgContext _context;

        public TaskCommentRepository(
            UtgContext context)
        {
            _context = context;
        }


        public async Task<TaskComment> CreateComment(TaskComment taskComment)
        {
            await _context.TaskComments.AddAsync(taskComment);
            await _context.SaveChangesAsync();
            return taskComment;
        }

        public IQueryable<TaskComment> Get()
        {
            return _context.TaskComments.AsQueryable();
        }
    }
}