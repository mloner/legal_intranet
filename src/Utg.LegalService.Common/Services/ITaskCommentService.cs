using System.Collections.Generic;
using System.Threading.Tasks;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.Common.Models.Request.TaskComments;

namespace Utg.LegalService.Common.Services
{
    public interface ITaskCommentService
    {
        Task<List<TaskCommentModel>> GetByTaskId(int taskId);
        Task CreateTaskComment(TaskCommentCreateRequest request, AuthInfo authInfo);
    }
}