using System;
using System.ComponentModel.DataAnnotations;
using Utg.Common.Models.Domain;

namespace Utg.LegalService.Common.Models.Domain
{
    public class TaskComment : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserProfileId { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }

        public virtual Task Task { get; set; }
    }
}