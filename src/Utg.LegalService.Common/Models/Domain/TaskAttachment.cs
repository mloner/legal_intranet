using System.ComponentModel.DataAnnotations;

namespace Utg.LegalService.Common.Models.Domain
{
    public class TaskAttachment : CustomFileModel
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int? UserProfileId { get; set; }

        public virtual Task Task { get; set; }
    }
}