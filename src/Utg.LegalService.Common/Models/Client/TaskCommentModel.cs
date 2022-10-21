using System;

namespace Utg.LegalService.Common.Models.Client
{
    public class TaskCommentModel
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserProfileId { get; set; }
        public string UserProfileFullName { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }
    }
}