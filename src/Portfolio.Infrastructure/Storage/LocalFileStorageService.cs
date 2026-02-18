using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public async Task<(string relativePath, string storedFileName)> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var yearMonth = $"{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}";
        var relativePath = $"/{_options.UploadDirectory}/{yearMonth}/{storedFileName}";
        var physicalPath = Path.Combine(_environment.WebRootPath, _options.UploadDirectory, yearMonth);

        Directory.CreateDirectory(physicalPath);

        var fullPath = Path.Combine(physicalPath, storedFileName);
        await using var fileStreamOut = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fileStreamOut, ct);

        return (relativePath, storedFileName);
    }

    public async Task DeleteFileAsync(string relativePath, CancellationToken ct = default)
    {
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(physicalPath))
        {
            await Task.Run(() => File.Delete(physicalPath), ct);
        }
    }

    public Task<bool> FileExistsAsync(string relativePath, CancellationToken ct = default)
    {
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        return Task.FromResult(File.Exists(physicalPath));
    }

    public string GetFileUrl(string relativePath)
    {
        return relativePath; // Client will use relative path
    }
}
