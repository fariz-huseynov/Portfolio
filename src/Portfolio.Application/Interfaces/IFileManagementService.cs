using Microsoft.AspNetCore.Http;
using Portfolio.Application.DTOs;

namespace Portfolio.Application.Interfaces;

public interface IFileManagementService
{
    Task<FileUploadResponseDto> UploadFileAsync(
        IFormFile file,
        string userId,
        CancellationToken ct = default);

    Task<FileMetadataDto> GetFileByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<FileMetadataDto>> GetAllFilesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task DeleteFileAsync(Guid id, CancellationToken ct = default);
}
