using System;
using AutoMapper;
using Utg.Common.EF.Repositories.Implementations;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal;

public class UnitOfWork : UnitOfWorkBase<UtgContext>, IUnitOfWork
{
    private readonly Lazy<IUserProfileAgregateRepository> _userProfileAgregateItems;
    public IUserProfileAgregateRepository UserProfileAgregatesRepository => _userProfileAgregateItems.Value;

    private readonly Lazy<ITaskRepository> _taskItems;
    public ITaskRepository TaskRepository => _taskItems.Value;
    
    private readonly Lazy<ITaskAttachmentRepository> _taskAttachmentItems;
    public ITaskAttachmentRepository TaskAttachmentRepository => _taskAttachmentItems.Value;
    
    private readonly Lazy<ITaskCommentRepository> _taskCommentItems;
    public ITaskCommentRepository TaskCommentRepository => _taskCommentItems.Value;
    
    private readonly Lazy<ITaskChangeHistoryRepository> _taskChangeHistoryItems;
    public ITaskChangeHistoryRepository TaskChangeHistoryRepository => _taskChangeHistoryItems.Value;
    
    public UnitOfWork(
        UtgContext dbContext,
        IMapper mapper) 
        : base(dbContext)
    {
        _userProfileAgregateItems = 
            new Lazy<IUserProfileAgregateRepository>(() => 
                new UserProfileAgregateRepository(dbContext));

        _taskItems = 
            new Lazy<ITaskRepository>(() =>
                new TaskRepository(dbContext, mapper));
        _taskAttachmentItems = 
            new Lazy<ITaskAttachmentRepository>(() => 
                new TaskAttachmentRepository(dbContext, mapper));
        _taskCommentItems = 
            new Lazy<ITaskCommentRepository>(() => 
                new TaskCommentRepository(dbContext));
        _taskChangeHistoryItems = 
            new Lazy<ITaskChangeHistoryRepository>(() => 
                new TaskChangeHistoryRepository(dbContext));
    }
}