namespace Portfolio.Domain.Entities;

public class FileMetadata : BaseEntity
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
