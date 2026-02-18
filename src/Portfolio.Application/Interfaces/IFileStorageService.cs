namespace Portfolio.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string relativePath, string storedFileName)> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default);

    Task DeleteFileAsync(string relativePath, CancellationToken ct = default);

    Task<bool> FileExistsAsync(string relativePath, CancellationToken ct = default);

    string GetFileUrl(string relativePath);
}
