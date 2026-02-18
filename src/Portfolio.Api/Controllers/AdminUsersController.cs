using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Authorization;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.DTOs.Users;
using Portfolio.Application.Interfaces;
using Asp.Versioning;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    [HttpGet("paged")]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetAllPaged(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _userService.GetAllUsersPagedAsync(pagination, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userService.GetUserByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [HasPermission(Permissions.UsersCreate)]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var user = await _userService.CreateUserAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<ActionResult<UserDto>> Update(
        Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.UpdateUserAsync(id, dto, currentUserId, ct);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.UsersDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.DeleteUserAsync(id, currentUserId, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/reset-password")]
    [HasPermission(Permissions.UsersResetPassword)]
    public async Task<IActionResult> ResetPassword(
        Guid id, [FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        await _userService.ResetPasswordAsync(id, dto, ct);
        return NoContent();
    }
}
