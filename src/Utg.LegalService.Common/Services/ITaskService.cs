using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Request;
using Utg.LegalService.Common.Models.Request.TaskComments;
using Utg.LegalService.Common.Models.Request.Tasks;

namespace Utg.LegalService.Common.Services
{
    public interface ITaskService
    {
        Task<TaskModel> GetById(int id, AuthInfo authInfo);
        Task<TaskModel> CreateTask(TaskCreateRequest request, AuthInfo authInfo);
        Task<TaskModel> UpdateTask(TaskUpdateRequest request, AuthInfo authInfo);
        Task DeleteTask(int id);
        Task<IEnumerable<UserProfileApiModel>> GetAuthorUserProfiles();
        Task<Stream> GetReport(GetTaskPageReportRequest request, AuthInfo authInfo);
        Task<TaskModel> UpdateTaskMoveToInWork(TaskUpdateMoveToInWorkRequest request, AuthInfo authInfo);
        Task<TaskModel> UpdateTaskMoveToUnderReview(TaskUpdateMoveToUnderReviewRequest request, AuthInfo authInfo);
        Task<TaskModel> UpdateTaskMoveToDone(TaskUpdateMoveToDoneRequest request, AuthInfo authInfo);
        Task<IEnumerable<UserProfileApiModel>> GetPerformerUserProfiles(AuthInfo authInfo);
        Task UploadFile(TaskUploadFileRequest request, AuthInfo authInfo);
        Task<TaskAttachmentModel> DownloadFile(int attachmentId);
        Task DeleteFile(int attachmentId);
        Task<UserProfileApiModel> GetUserProfileById(int id);
    }
}