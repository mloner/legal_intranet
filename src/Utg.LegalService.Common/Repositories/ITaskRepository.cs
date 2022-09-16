using System;
using System.Linq;
using System.Threading.Tasks;
using Utg.LegalService.Common.Models.Client;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskRepository
    {
        IQueryable<TaskModel> Get();
    }
}
