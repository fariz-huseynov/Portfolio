namespace Portfolio.Application.Common;

public class FileStorageOptions
{
    public string UploadDirectory { get; set; } = "uploads";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif"];
    public string[] AllowedDocumentExtensions { get; set; } = [".pdf"];
}
