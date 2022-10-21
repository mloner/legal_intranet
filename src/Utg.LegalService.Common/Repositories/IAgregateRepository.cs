using System.Collections.Generic;
using System.Linq;
using Utg.LegalService.Common.Models.Domain;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Common.Repositories;

public interface IAgregateRepository
{
    IQueryable<UserProfileAgregate> GetUserProfiles();
    Task AddUserProfiles(IEnumerable<UserProfileAgregate> userProfileAgregates);
}