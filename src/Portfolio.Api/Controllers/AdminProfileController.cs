using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.DTOs.Users;
using Asp.Versioning;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/profile")]
[Authorize]
public class AdminProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileManagementService _fileService;

    public AdminProfileController(
        IUserService userService,
        IFileManagementService fileService)
    {
        _userService = userService;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetProfile(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _userService.GetProfileAsync(userId, ct);
        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateProfile(
        [FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _userService.UpdateProfileAsync(userId, dto, ct);
        return Ok(profile);
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit for avatars
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UploadAvatar(
        IFormFile file,
        CancellationToken ct)
    {
        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Only image files (JPEG, PNG, GIF, WebP) are allowed for avatars." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Upload file to storage
        var uploadResult = await _fileService.UploadFileAsync(file, userId, ct);

        // Update user's avatar URL
        var updateDto = new UpdateProfileDto
        {
            FullName = (await _userService.GetProfileAsync(Guid.Parse(userId), ct)).FullName,
            AvatarUrl = uploadResult.Url
        };

        var profile = await _userService.UpdateProfileAsync(Guid.Parse(userId), updateDto, ct);
        return Ok(profile);
    }

    [HttpDelete("avatar")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> RemoveAvatar(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get current profile to preserve other data
        var currentProfile = await _userService.GetProfileAsync(userId, ct);

        // Update with null avatar
        var updateDto = new UpdateProfileDto
        {
            FullName = currentProfile.FullName,
            AvatarUrl = null
        };

        var profile = await _userService.UpdateProfileAsync(userId, updateDto, ct);
        return Ok(profile);
    }
}
