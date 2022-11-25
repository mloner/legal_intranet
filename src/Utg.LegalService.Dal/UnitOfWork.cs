using System;
using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal;

public class UnitOfWork : UnitOfWorkBase<UtgContext>
{
    private readonly Lazy<IAgregateRepository> _agregateItems;
    public IAgregateRepository AgregateItems => _agregateItems.Value;
    
    public UnitOfWork(
        UtgContext dbContext) 
        : base(dbContext)
    {
        _agregateItems = new Lazy<IAgregateRepository>(() => new AgregateRepository(dbContext));
    }
}