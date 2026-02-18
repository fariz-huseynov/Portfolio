using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Portfolio.Api.Authorization;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Asp.Versioning;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/projects")]
public class AdminProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IOutputCacheStore _outputCacheStore;

    public AdminProjectsController(IProjectService projectService, IOutputCacheStore outputCacheStore)
    {
        _projectService = projectService;
        _outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [HasPermission(Permissions.ProjectsView)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken ct)
    {
        var projects = await _projectService.GetAllProjectsAsync(ct);
        return Ok(projects);
    }

    [HttpGet("paged")]
    [HasPermission(Permissions.ProjectsView)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetAllPaged(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _projectService.GetAllProjectsPagedAsync(pagination, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.ProjectsView)]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken ct)
    {
        var projects = await _projectService.GetAllProjectsAsync(ct);
        var found = projects.FirstOrDefault(p => p.Id == id);
        return found is null ? NotFound() : Ok(found);
    }

    [HttpPost]
    [HasPermission(Permissions.ProjectsCreate)]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] ProjectCreateDto dto, CancellationToken ct)
    {
        var project = await _projectService.CreateProjectAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.ProjectsEdit)]
    public async Task<ActionResult<ProjectDto>> Update(
        Guid id, [FromBody] ProjectUpdateDto dto, CancellationToken ct)
    {
        var project = await _projectService.UpdateProjectAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(project);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ProjectsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _projectService.DeleteProjectAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }
}
