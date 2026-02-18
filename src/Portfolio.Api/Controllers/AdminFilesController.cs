using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Authorization;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using System.Security.Claims;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/files")]
[Authorize]
[HasPermission("Files.Manage")]
public class AdminFilesController : ControllerBase
{
    private readonly IFileManagementService _fileService;

    public AdminFilesController(IFileManagementService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    [ProducesResponseType(typeof(FileUploadResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _fileService.UploadFileAsync(file, userId, ct);
        return CreatedAtAction(nameof(GetFileById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FileMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileById(Guid id, CancellationToken ct = default)
    {
        var file = await _fileService.GetFileByIdAsync(id, ct);
        return Ok(file);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FileMetadataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFiles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var files = await _fileService.GetAllFilesAsync(pageNumber, pageSize, ct);
        return Ok(files);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(Guid id, CancellationToken ct = default)
    {
        await _fileService.DeleteFileAsync(id, ct);
        return NoContent();
    }
}
