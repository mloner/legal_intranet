using System.Collections.Generic;
using System.Linq;
using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Dal.Repositories;

public class AgregateRepository 
    : BaseRepository<UserProfileAgregate>, IAgregateRepository
{
    private readonly UtgContext _context;

    public AgregateRepository(UtgContext context)
        : base(context)
    {
        _context = context;
    }

    public IQueryable<UserProfileAgregate> Get()
    {
        return _context.UserProfileAgregates.AsQueryable();
    }

    public async System.Threading.Tasks.Task AddUserProfiles(IEnumerable<UserProfileAgregate> userProfileAgregates)
    {
        await _context.UserProfileAgregates.AddRangeAsync(userProfileAgregates);
        await _context.SaveChangesAsync();
    }
}