using System;
using AutoMapper;
using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal;

public class UnitOfWork : UnitOfWorkBase<UtgContext>
{
    private readonly Lazy<ITaskRepository> _taskItems;
    public ITaskRepository TaskItems => _taskItems.Value;
    
    private readonly Lazy<ITaskAttachmentRepository> _taskAttachmentItems;
    public ITaskAttachmentRepository TaskAttachmentItems => _taskAttachmentItems.Value;
    
    public UnitOfWork(
        UtgContext dbContext,
        IMapper mapper) 
        : base(dbContext)
    {
        _taskItems = 
            new Lazy<ITaskRepository>(() =>
                new TaskRepository(dbContext, mapper));
        _taskAttachmentItems = 
            new Lazy<ITaskAttachmentRepository>(() => 
                new TaskAttachmentRepository(dbContext, mapper));
    }
}