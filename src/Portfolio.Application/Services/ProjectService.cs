using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IHybridCacheService _cache;
    private readonly TimeSpan _slugCacheDuration;
    private readonly TimeSpan _listCacheDuration;

    public ProjectService(IProjectRepository projectRepository, IHtmlSanitizerService htmlSanitizer, IHybridCacheService cache, IOptions<CachingOptions> cachingOptions)
    {
        _projectRepository = projectRepository;
        _htmlSanitizer = htmlSanitizer;
        _cache = cache;
        _slugCacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.ProjectMinutes);
        _listCacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.PublishedListMinutes);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetPublishedProjectsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.ProjectsPublished,
            async () =>
            {
                var projects = await _projectRepository.GetPublishedAsync(ct);
                return (IReadOnlyList<ProjectDto>)projects.Select(MapToDto).ToList();
            },
            _listCacheDuration,
            null,
            ct
        ) ?? new List<ProjectDto>();
    }

    public async Task<PagedResult<ProjectDto>> GetPublishedProjectsPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var (items, totalCount) = await _projectRepository.GetPublishedPagedAsync(pagination.Page, pagination.PageSize, ct);
        return PagedResult<ProjectDto>.Create(items.Select(MapToDto).ToList(), pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<ProjectDto?> GetProjectBySlugAsync(string slug, CancellationToken ct = default)
    {
        var cached = await _cache.GetOrCreateAsync(
            CacheKeys.ProjectBySlug(slug),
            async () =>
            {
                var project = await _projectRepository.GetBySlugAsync(slug, ct);
                return project is not null ? MapToDto(project) : null!;
            },
            _slugCacheDuration,
            null,
            ct
        );
        return cached;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllProjectsAsync(CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetAllAsync(ct);
        return projects.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<ProjectDto>> GetAllProjectsPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var (items, totalCount) = await _projectRepository.GetPagedAsync(pagination.Page, pagination.PageSize, ct);
        return PagedResult<ProjectDto>.Create(items.Select(MapToDto).ToList(), pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, CancellationToken ct = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Slug = dto.Slug,
            Summary = dto.Summary,
            Description = _htmlSanitizer.Sanitize(dto.Description),
            ThumbnailUrl = dto.ThumbnailUrl,
            LiveUrl = dto.LiveUrl,
            GitHubUrl = dto.GitHubUrl,
            TechStack = dto.TechStack,
            SortOrder = dto.SortOrder,
            IsPublished = dto.IsPublished,
            PublishedAt = dto.IsPublished ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project, ct);
        await _cache.RemoveAsync(CacheKeys.ProjectsPublished, ct);
        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, ProjectUpdateDto dto, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Project with ID {id} not found.");

        var wasPublished = project.IsPublished;

        project.Title = dto.Title;
        project.Slug = dto.Slug;
        project.Summary = dto.Summary;
        project.Description = _htmlSanitizer.Sanitize(dto.Description);
        project.ThumbnailUrl = dto.ThumbnailUrl;
        project.LiveUrl = dto.LiveUrl;
        project.GitHubUrl = dto.GitHubUrl;
        project.TechStack = dto.TechStack;
        project.SortOrder = dto.SortOrder;
        project.IsPublished = dto.IsPublished;
        project.UpdatedAt = DateTime.UtcNow;

        if (!wasPublished && dto.IsPublished)
            project.PublishedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, ct);
        await _cache.RemoveAsync(CacheKeys.ProjectsPublished, ct);
        await _cache.RemoveAsync(CacheKeys.ProjectBySlug(dto.Slug), ct);
        return MapToDto(project);
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Project with ID {id} not found.");

        await _projectRepository.DeleteAsync(project, ct);
        await _cache.RemoveAsync(CacheKeys.ProjectsPublished, ct);
        await _cache.RemoveAsync(CacheKeys.ProjectBySlug(project.Slug), ct);
    }

    private static ProjectDto MapToDto(Project p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Summary = p.Summary,
        Description = p.Description,
        ThumbnailUrl = p.ThumbnailUrl,
        LiveUrl = p.LiveUrl,
        GitHubUrl = p.GitHubUrl,
        TechStack = p.TechStack,
        SortOrder = p.SortOrder,
        IsPublished = p.IsPublished,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt
    };
}
