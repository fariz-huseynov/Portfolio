using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Authorization;
using Portfolio.Application.DTOs.Roles;
using Portfolio.Application.Interfaces;
using Asp.Versioning;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/roles")]
public class AdminRolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public AdminRolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetAll(CancellationToken ct)
    {
        var roles = await _roleService.GetAllRolesAsync(ct);
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<RoleDto>> GetById(Guid id, CancellationToken ct)
    {
        var role = await _roleService.GetRoleByIdAsync(id, ct);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    [HasPermission(Permissions.UsersCreate)]
    public async Task<ActionResult<RoleDto>> Create(
        [FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        var role = await _roleService.CreateRoleAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<ActionResult<RoleDto>> Update(
        Guid id, [FromBody] UpdateRoleDto dto, CancellationToken ct)
    {
        var role = await _roleService.UpdateRoleAsync(id, dto, ct);
        return Ok(role);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.UsersDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _roleService.DeleteRoleAsync(id, ct);
        return NoContent();
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> GetPermissions(CancellationToken ct)
    {
        var permissions = await _roleService.GetAllPermissionsAsync(ct);
        return Ok(permissions);
    }
}
