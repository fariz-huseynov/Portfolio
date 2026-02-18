namespace Portfolio.Application.DTOs;

public record FileUploadResponseDto
{
    public required Guid Id { get; init; }
    public required string OriginalFileName { get; init; }
    public required string Url { get; init; }
    public required long FileSizeBytes { get; init; }
    public required string ContentType { get; init; }
}
