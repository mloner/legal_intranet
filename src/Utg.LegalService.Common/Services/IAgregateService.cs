using System.Collections.Generic;
using System.Threading.Tasks;
using Utg.LegalService.Common.Models.Client;

namespace Utg.LegalService.Common.Services;

public interface IAgregateService
{
    Task FillUserProfiles();
    Task<IEnumerable<UserProfileAgregateModel>> GetUserProfiles(IEnumerable<int> ids);
}