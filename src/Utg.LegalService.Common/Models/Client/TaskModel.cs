using System;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Client
{
    public class TaskModel
    {
        public int Id { get; set; }
        public TaskStatus Status { get; set; }
        public string StatusName => Status.GetDisplayName();
        public TaskType Type { get; set; }
        public string TypeName => Type.GetDisplayName();
        public string Description { get; set; }
        public int AuthorUserProfileId { get; set; }
        public string AuthorFullName { get; set; }
        public DateTime CreationDateTime { get; set; }
        public int PerformerUserProfileId { get; set; }
        public string PerformerFullName { get; set; }
        public DateTime DeadlineDateTime { get; set; }
        public DateTime LastChangeDateTime { get; set; }
    }
}