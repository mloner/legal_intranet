using System.Collections.Generic;
using System.Linq;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Dal.Repositories;

public class AgregateRepository : IAgregateRepository
{
    private readonly UtgContext _context;

    public AgregateRepository(UtgContext context)
    {
        _context = context;
    }

    public IQueryable<UserProfileAgregate> GetUserProfiles()
    {
        return _context.UserProfileAgregates.AsQueryable();
    }

    public async Task AddUserProfiles(IEnumerable<UserProfileAgregate> userProfileAgregates)
    {
        await _context.UserProfileAgregates.AddRangeAsync(userProfileAgregates);
        await _context.SaveChangesAsync();
    }
}