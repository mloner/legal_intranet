using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal.Repositories;

public class TaskChangeHistoryRepository 
    : BaseRepository<UtgContext, TaskChangeHistory>, ITaskChangeHistoryRepository
{
    public TaskChangeHistoryRepository(UtgContext context)
        : base(context)
    {
    }
}