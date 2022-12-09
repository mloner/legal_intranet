using System.Collections.Generic;
using System.Linq;
using Utg.Common.EF.Repositories;
using Utg.LegalService.Common.Models.Domain;

namespace Utg.LegalService.Common.Repositories;

public interface ITaskChangeHistoryRepository 
    : IBaseRepositoryAdvanced<TaskChangeHistory>
{
}