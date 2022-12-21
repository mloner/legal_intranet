using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utg.Common.EF.Repositories;
using Utg.LegalService.Common.Models.Domain;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Common.Repositories;

public interface IUserProfileAgregateRepository 
    : IBaseRepositoryAdvanced<UserProfileAgregate>
{
    Task AddUserProfilesAsync(IEnumerable<UserProfileAgregate> userProfileAgregates,
        CancellationToken cancellationToken = default);

    IQueryable<UserProfileAgregate> Get();
    Task RemoveAllAsync(CancellationToken cancellationToken = default);

    Task<UserProfileAgregate> GetUserProfileAgregateByUserProfileId(int userProfileId,
        CancellationToken cancellationToken = default);
}