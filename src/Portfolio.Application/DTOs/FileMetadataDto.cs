namespace Portfolio.Application.DTOs;

public record FileMetadataDto
{
    public required Guid Id { get; init; }
    public required string OriginalFileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSizeBytes { get; init; }
    public required string Url { get; init; }
    public required string FileExtension { get; init; }
    public required DateTime UploadedAt { get; init; }
    public required string UploadedByUserId { get; init; }
}
