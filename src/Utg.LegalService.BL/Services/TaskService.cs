using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Utg.Common.Packages.Domain;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.LegalService.Common;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Common.Services;
using TaskStatus = Utg.LegalService.Common.Models.Client.Enum.TaskStatus;

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

            query = Filter(query, request);
            query = Search(query, request);
            
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
    }
}