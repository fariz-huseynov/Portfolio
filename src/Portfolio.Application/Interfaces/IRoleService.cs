using Portfolio.Application.DTOs.Roles;

namespace Portfolio.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllRolesAsync(CancellationToken ct = default);
    Task<RoleDto?> GetRoleByIdAsync(Guid id, CancellationToken ct = default);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto, CancellationToken ct = default);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto, CancellationToken ct = default);
    Task DeleteRoleAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken ct = default);
}
