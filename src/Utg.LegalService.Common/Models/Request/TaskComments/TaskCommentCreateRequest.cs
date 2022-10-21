namespace Utg.LegalService.Common.Models.Request.TaskComments
{
    public class TaskCommentCreateRequest
    {
        public int TaskId { get; set; }
        public string Text { get; set; }
    }
}