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
[Route("api/v{version:apiVersion}/admin/blogs")]
public class AdminBlogsController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IOutputCacheStore _outputCacheStore;

    public AdminBlogsController(IBlogPostService blogPostService, IOutputCacheStore outputCacheStore)
    {
        _blogPostService = blogPostService;
        _outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [HasPermission(Permissions.BlogsView)]
    public async Task<ActionResult<IReadOnlyList<BlogPostDto>>> GetAll(CancellationToken ct)
    {
        var posts = await _blogPostService.GetAllPostsAsync(ct);
        return Ok(posts);
    }

    [HttpGet("paged")]
    [HasPermission(Permissions.BlogsView)]
    public async Task<ActionResult<PagedResult<BlogPostDto>>> GetAllPaged(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _blogPostService.GetAllPostsPagedAsync(pagination, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.BlogsView)]
    public async Task<ActionResult<BlogPostDto>> GetById(Guid id, CancellationToken ct)
    {
        var posts = await _blogPostService.GetAllPostsAsync(ct);
        var found = posts.FirstOrDefault(p => p.Id == id);
        return found is null ? NotFound() : Ok(found);
    }

    [HttpPost]
    [HasPermission(Permissions.BlogsCreate)]
    public async Task<ActionResult<BlogPostDto>> Create(
        [FromBody] BlogPostCreateDto dto, CancellationToken ct)
    {
        var post = await _blogPostService.CreatePostAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.BlogsEdit)]
    public async Task<ActionResult<BlogPostDto>> Update(
        Guid id, [FromBody] BlogPostUpdateDto dto, CancellationToken ct)
    {
        var post = await _blogPostService.UpdatePostAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(post);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.BlogsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _blogPostService.DeletePostAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    [HasPermission(Permissions.BlogsEdit)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await _blogPostService.PublishPostAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unpublish")]
    [HasPermission(Permissions.BlogsEdit)]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
    {
        await _blogPostService.UnpublishPostAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }
}
