using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;

namespace Portfolio.Application.Interfaces;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetPublishedProjectsAsync(CancellationToken ct = default);
    Task<PagedResult<ProjectDto>> GetPublishedProjectsPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<ProjectDto?> GetProjectBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetAllProjectsAsync(CancellationToken ct = default);
    Task<PagedResult<ProjectDto>> GetAllProjectsPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, CancellationToken ct = default);
    Task<ProjectDto> UpdateProjectAsync(Guid id, ProjectUpdateDto dto, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid id, CancellationToken ct = default);
}
