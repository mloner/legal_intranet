using System;
using System.ComponentModel.DataAnnotations;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Domain
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public TaskStatus Status { get; set; }
        public TaskType Type { get; set; }
        public string AuthorUserProfileId { get; set; }
        public string AuthorFullName { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string PerformerUserProfileId { get; set; }
        public string PerformerFullName { get; set; }
        public DateTime DeadlineDateTime { get; set; }
        public DateTime LastChangeDateTime { get; set; }
    }
}