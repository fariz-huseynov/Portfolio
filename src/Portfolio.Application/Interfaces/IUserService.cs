using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.DTOs.Users;

namespace Portfolio.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetAllUsersPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto, Guid currentUserId, CancellationToken ct = default);
    Task DeleteUserAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
    Task ResetPasswordAsync(Guid id, ResetPasswordDto dto, CancellationToken ct = default);
    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default);
}
