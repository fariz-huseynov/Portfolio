using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;
using Asp.Versioning;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/portfolio")]
[AllowAnonymous]
[EnableRateLimiting("PublicApi")]
public class PublicPortfolioController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IBlogPostService _blogPostService;

    public PublicPortfolioController(
        IProjectService projectService,
        IBlogPostService blogPostService)
    {
        _projectService = projectService;
        _blogPostService = blogPostService;
    }

    [HttpGet("projects")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetProjects(CancellationToken ct)
    {
        var projects = await _projectService.GetPublishedProjectsAsync(ct);
        return Ok(projects);
    }

    [HttpGet("projects/paged")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjectsPaged(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _projectService.GetPublishedProjectsPagedAsync(pagination, ct);
        return Ok(result);
    }

    [HttpGet("projects/{slug}")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<ProjectDto>> GetProjectBySlug(string slug, CancellationToken ct)
    {
        var project = await _projectService.GetProjectBySlugAsync(slug, ct);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpGet("blogs")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<BlogPostDto>>> GetBlogs(CancellationToken ct)
    {
        var posts = await _blogPostService.GetPublishedPostsAsync(ct);
        return Ok(posts);
    }

    [HttpGet("blogs/paged")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<PagedResult<BlogPostDto>>> GetBlogsPaged(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _blogPostService.GetPublishedPostsPagedAsync(pagination, ct);
        return Ok(result);
    }

    [HttpGet("blogs/{slug}")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<BlogPostDto>> GetBlogBySlug(string slug, CancellationToken ct)
    {
        var post = await _blogPostService.GetPostBySlugAsync(slug, ct);
        return post is null ? NotFound() : Ok(post);
    }
}
