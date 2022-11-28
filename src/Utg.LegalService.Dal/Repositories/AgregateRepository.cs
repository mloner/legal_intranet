using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Dal.Repositories;

public class AgregateRepository : BaseRepositoryAdvanced<UtgContext, UserProfileAgregate>, IAgregateRepository
{
    public AgregateRepository(UtgContext context) : base(context)
    {
    }
}