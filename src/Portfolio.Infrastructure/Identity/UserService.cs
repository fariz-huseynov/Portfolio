using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.DTOs.Users;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync(ct);
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(MapToDto(user, roles));
        }
        return result;
    }

    public async Task<PagedResult<UserDto>> GetAllUsersPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _userManager.Users.OrderByDescending(u => u.CreatedAt);
        var totalCount = await query.CountAsync(ct);
        var users = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(MapToDto(user, roles));
        }
        return PagedResult<UserDto>.Create(result, pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        if (dto.RoleIds.Count > 0)
        {
            var roleNames = await _roleManager.Roles
                .Where(r => dto.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(ct);
            await _userManager.AddToRolesAsync(user, roleNames);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto, Guid currentUserId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        if (id == currentUserId && !dto.IsActive)
            throw new InvalidOperationException("You cannot disable your own account.");

        user.FullName = dto.FullName;
        user.AvatarUrl = dto.AvatarUrl;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // Update roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        var targetRoleNames = await _roleManager.Roles
            .Where(r => dto.RoleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync(ct);

        var rolesToRemove = currentRoles.Except(targetRoleNames).ToList();
        var rolesToAdd = targetRoleNames.Except(currentRoles).ToList();

        if (rolesToRemove.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        if (rolesToAdd.Count > 0)
            await _userManager.AddToRolesAsync(user, rolesToAdd);

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task DeleteUserAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
            throw new InvalidOperationException("You cannot delete your own account.");

        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        user.FullName = dto.FullName;
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        FullName = user.FullName,
        AvatarUrl = user.AvatarUrl,
        IsActive = user.IsActive,
        Roles = roles.ToList(),
        CreatedAt = user.CreatedAt
    };
}
