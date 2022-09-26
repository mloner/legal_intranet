using System;

namespace Utg.LegalService.Common.Models
{
    public class CustomFileModel
    {
        public string FileName { get; set; } = default!;

        public Guid FileId { get; set; }

        public long FileSizeInBytes { get; set; }
    }
}