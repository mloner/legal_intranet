namespace Utg.LegalService.Common.Models.Client
{
    public class TaskAttachmentModel : CustomFileModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskModel Task { get; set; }
    }
}