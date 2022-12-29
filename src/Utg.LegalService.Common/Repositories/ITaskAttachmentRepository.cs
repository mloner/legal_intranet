using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utg.Common.EF.Repositories;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Domain;
using Task = System.Threading.Tasks.Task;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskAttachmentRepository 
        : IBaseRepositoryAdvanced<TaskAttachment>
    {
        IQueryable<TaskAttachment> Get();
        Task<IEnumerable<TaskAttachmentModel>>  CreateAttachments(int taskId, IEnumerable<TaskAttachmentModel> attachments);
        Task RemoveAttachments(int taskId, IEnumerable<int> attachmentIds);
        Task Delete(TaskAttachment taskAttachment);
    }
}