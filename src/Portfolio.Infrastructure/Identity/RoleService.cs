using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.DTOs.Roles;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Identity;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AppDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        AppDbContext dbContext,
        IPermissionService permissionService)
    {
        _roleManager = roleManager;
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllRolesAsync(CancellationToken ct = default)
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync(ct);
        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissions = await GetPermissionsForRoleAsync(role.Id, ct);
            result.Add(MapToDto(role, permissions));
        }
        return result;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null) return null;
        var permissions = await GetPermissionsForRoleAsync(role.Id, ct);
        return MapToDto(role, permissions);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto, CancellationToken ct = default)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        if (dto.PermissionIds.Count > 0)
        {
            var rolePermissions = dto.PermissionIds.Select(pid => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = pid
            });
            _dbContext.RolePermissions.AddRange(rolePermissions);
            await _dbContext.SaveChangesAsync(ct);
        }

        _permissionService.InvalidateCache();

        var permissions = await GetPermissionsForRoleAsync(role.Id, ct);
        return MapToDto(role, permissions);
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException($"Role with ID {id} not found.");

        role.Name = dto.Name;
        role.Description = dto.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // Replace permissions
        var existingPermissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == id)
            .ToListAsync(ct);
        _dbContext.RolePermissions.RemoveRange(existingPermissions);

        if (dto.PermissionIds.Count > 0)
        {
            var rolePermissions = dto.PermissionIds.Select(pid => new RolePermission
            {
                RoleId = id,
                PermissionId = pid
            });
            _dbContext.RolePermissions.AddRange(rolePermissions);
        }

        await _dbContext.SaveChangesAsync(ct);
        _permissionService.InvalidateCache();

        var permissions = await GetPermissionsForRoleAsync(id, ct);
        return MapToDto(role, permissions);
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException($"Role with ID {id} not found.");

        // Remove role-permission mappings first
        var rolePermissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == id)
            .ToListAsync(ct);
        _dbContext.RolePermissions.RemoveRange(rolePermissions);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        _permissionService.InvalidateCache();
    }

    public async Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _dbContext.Permissions.OrderBy(p => p.Name).ToListAsync(ct);
        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        }).ToList();
    }

    private async Task<List<PermissionDto>> GetPermissionsForRoleAsync(Guid roleId, CancellationToken ct)
    {
        return await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description
            })
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    private static RoleDto MapToDto(ApplicationRole role, List<PermissionDto> permissions) => new()
    {
        Id = role.Id,
        Name = role.Name!,
        Description = role.Description,
        Permissions = permissions,
        CreatedAt = role.CreatedAt
    };
}
