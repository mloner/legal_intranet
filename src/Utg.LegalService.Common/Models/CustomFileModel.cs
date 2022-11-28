using System;
using Utg.Common.Models.Domain;

namespace Utg.LegalService.Common.Models
{
    public class CustomFileModel : BaseEntity
    {
        public string FileName { get; set; } = default!;

        public Guid FileId { get; set; }

        public long FileSizeInBytes { get; set; }
    }
}