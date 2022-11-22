using System.IO;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.Common.Models.Client.Attachment
{
    public class TaskAttachmentModel : CustomFileModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int? UserProfileId { get; set; }
        public Stream Bytes { get; set; }
        public TaskModel Task { get; set; }
        public AttachmentAccessRights AccessRights { get; set; }
    }
}