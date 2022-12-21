using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal.Repositories;

public class UserProfileAgregateRepository 
    : BaseRepositoryAdvanced<UtgContext, UserProfileAgregate>, IUserProfileAgregateRepository
{
    private readonly UtgContext _context;

    public UserProfileAgregateRepository(UtgContext context)
        : base(context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AddUserProfilesAsync(IEnumerable<UserProfileAgregate> userProfileAgregates,
        CancellationToken cancellationToken = default)
    {
        await _context.UserProfileAgregates.AddRangeAsync(userProfileAgregates,
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<UserProfileAgregate> Get()
    {
        return _context.UserProfileAgregates.AsQueryable();
    }

    public System.Threading.Tasks.Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        _context.UserProfileAgregates.RemoveRange(_context.UserProfileAgregates);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<UserProfileAgregate> GetUserProfileAgregateByUserProfileId(int userProfileId, CancellationToken cancellationToken = default)
    {
        return await  _context.UserProfileAgregates
            .FirstOrDefaultAsync(q => q.UserProfileId == userProfileId, cancellationToken);
    }
}