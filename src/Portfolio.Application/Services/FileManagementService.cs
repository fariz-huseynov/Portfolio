using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class FileManagementService : IFileManagementService
{
    private readonly IRepository<FileMetadata> _fileRepo;
    private readonly IFileStorageService _storage;
    private readonly FileStorageOptions _options;

    public FileManagementService(
        IRepository<FileMetadata> fileRepo,
        IFileStorageService storage,
        IOptions<FileStorageOptions> options)
    {
        _fileRepo = fileRepo;
        _storage = storage;
        _options = options.Value;
    }

    public async Task<FileUploadResponseDto> UploadFileAsync(
        IFormFile file,
        string userId,
        CancellationToken ct = default)
    {
        // Step 1: Basic validation
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > _options.MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / 1024 / 1024} MB.");

        // Step 2: Sanitize and validate filename
        var sanitizedFilename = FileValidation.SanitizeFilename(file.FileName);

        // Check for double extension attacks (e.g., image.jpg.exe)
        if (FileValidation.HasDoubleExtension(sanitizedFilename))
            throw new ArgumentException("Files with multiple extensions are not allowed for security reasons.");

        var extension = Path.GetExtension(sanitizedFilename).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("File must have an extension.");

        // Step 3: Validate extension against whitelist
        var allAllowedExtensions = _options.AllowedImageExtensions
            .Concat(_options.AllowedDocumentExtensions)
            .ToArray();

        if (!FileValidation.IsAllowedExtension(extension, allAllowedExtensions))
            throw new ArgumentException($"File type '{extension}' is not allowed.");

        // Step 4: Validate MIME type matches extension
        var expectedMimeTypes = GetExpectedMimeTypes(extension);
        if (!expectedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException($"MIME type '{file.ContentType}' does not match file extension '{extension}'.");

        // Step 5: Validate file signature (magic bytes)
        await using var stream = file.OpenReadStream();
        if (!FileValidation.IsValidFileSignature(stream, extension))
            throw new ArgumentException("File signature validation failed. File may be corrupt, tampered with, or not a valid file type.");

        // Step 6: Scan for suspicious content (malware, scripts, executables)
        stream.Position = 0;
        if (FileValidation.ContainsSuspiciousContent(stream))
            throw new ArgumentException("File contains suspicious or potentially malicious content and has been rejected.");

        // Step 7: Save the validated file
        // NOTE: Files are copied as-is after validation. For maximum security in production,
        // consider integrating external virus scanning (ClamAV, VirusTotal API) or
        // image re-encoding to strip metadata and potential exploits.
        stream.Position = 0;
        var (relativePath, storedFileName) = await _storage.SaveFileAsync(
            stream,
            sanitizedFilename,
            file.ContentType,
            ct);

        // Create metadata
        var metadata = new FileMetadata
        {
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            RelativePath = relativePath,
            FileExtension = extension.TrimStart('.'),
            UploadedByUserId = userId,
            UploadedAt = DateTime.UtcNow
        };

        await _fileRepo.AddAsync(metadata, ct);

        return new FileUploadResponseDto
        {
            Id = metadata.Id,
            OriginalFileName = metadata.OriginalFileName,
            Url = _storage.GetFileUrl(relativePath),
            FileSizeBytes = metadata.FileSizeBytes,
            ContentType = metadata.ContentType
        };
    }

    public async Task<FileMetadataDto> GetFileByIdAsync(Guid id, CancellationToken ct = default)
    {
        var file = await _fileRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"File with ID {id} not found.");

        return MapToDto(file);
    }

    public async Task<IReadOnlyList<FileMetadataDto>> GetAllFilesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var files = await _fileRepo.GetAllAsync(ct);
        var paged = files
            .OrderByDescending(f => f.UploadedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return paged;
    }

    public async Task DeleteFileAsync(Guid id, CancellationToken ct = default)
    {
        var file = await _fileRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"File with ID {id} not found.");

        // Delete physical file
        await _storage.DeleteFileAsync(file.RelativePath, ct);

        // Delete metadata
        await _fileRepo.DeleteAsync(file, ct);
    }

    private FileMetadataDto MapToDto(FileMetadata file)
    {
        return new FileMetadataDto
        {
            Id = file.Id,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.FileSizeBytes,
            Url = _storage.GetFileUrl(file.RelativePath),
            FileExtension = file.FileExtension,
            UploadedAt = file.UploadedAt,
            UploadedByUserId = file.UploadedByUserId
        };
    }

    private static HashSet<string> GetExpectedMimeTypes(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/jpg"
            },
            ".png" => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/png"
            },
            ".gif" => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/gif"
            },
            ".pdf" => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf"
            },
            _ => new HashSet<string>()
        };
    }
}
