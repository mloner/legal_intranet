using System.Collections.Generic;
using System.Threading.Tasks;
using Utg.LegalService.Common.Models.Client;

namespace Utg.LegalService.Common.Repositories
{
    public interface ITaskAttachmentRepository
    {
        Task<IEnumerable<TaskAttachmentModel>>  CreateAttachments(int taskId, IEnumerable<TaskAttachmentModel> attachments);
        Task RemoveAttachments(int taskId, IEnumerable<int> attachmentIds);
    }
}