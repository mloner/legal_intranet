using Utg.Common.EF.Repositories;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal;

public interface IUnitOfWork : IUnitOfWorkBase<UtgContext>
{
    IUserProfileAgregateRepository UserProfileAgregatesRepository { get; }
    ITaskRepository TaskRepository { get; }
    ITaskAttachmentRepository TaskAttachmentRepository { get; }
    ITaskCommentRepository TaskCommentRepository { get; }
    ITaskChangeHistoryRepository TaskChangeHistoryRepository { get; }
}