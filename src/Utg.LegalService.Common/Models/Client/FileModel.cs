using System;

namespace Utg.LegalService.Common.Models.Client;

public class FileModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public Guid StorageFileId { get; set; }
}