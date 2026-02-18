using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Api.Authorization;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs.AiContent;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/ai")]
public class AdminAiController : ControllerBase
{
    private readonly IAiContentService _aiContentService;

    public AdminAiController(IAiContentService aiContentService)
    {
        _aiContentService = aiContentService;
    }

    /// <summary>
    /// Generate text content using AI (blog posts, descriptions, etc.)
    /// </summary>
    [HttpPost("generate-text")]
    [HasPermission(Permissions.AiContentGenerate)]
    [EnableRateLimiting(PolicyNames.RateLimitAiGeneration)]
    public async Task<ActionResult<AiGenerationResultDto>> GenerateText(
        [FromBody] AiGenerateTextRequestDto request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _aiContentService.GenerateTextAsync(request, userId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Rewrite existing text using AI
    /// </summary>
    [HttpPost("rewrite-text")]
    [HasPermission(Permissions.AiContentGenerate)]
    [EnableRateLimiting(PolicyNames.RateLimitAiGeneration)]
    public async Task<ActionResult<AiGenerationResultDto>> RewriteText(
        [FromBody] AiRewriteTextRequestDto request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _aiContentService.RewriteTextAsync(request, userId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Generate an image using AI
    /// </summary>
    [HttpPost("generate-image")]
    [HasPermission(Permissions.AiContentGenerate)]
    [EnableRateLimiting(PolicyNames.RateLimitAiGeneration)]
    public async Task<ActionResult<AiGenerationResultDto>> GenerateImage(
        [FromBody] AiGenerateImageRequestDto request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _aiContentService.GenerateImageAsync(request, userId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific generation result by ID
    /// </summary>
    [HttpGet("generations/{id:guid}")]
    [HasPermission(Permissions.AiContentView)]
    public async Task<ActionResult<AiGenerationResultDto>> GetGeneration(Guid id, CancellationToken ct)
    {
        var result = await _aiContentService.GetGenerationByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get AI generation history (paginated)
    /// </summary>
    [HttpGet("generations")]
    [HasPermission(Permissions.AiContentView)]
    public async Task<ActionResult<PagedResult<AiGenerationResultDto>>> GetGenerations(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _aiContentService.GetGenerationHistoryAsync(pagination, ct);
        return Ok(result);
    }

    /// <summary>
    /// List available AI providers and their configuration status
    /// </summary>
    [HttpGet("providers")]
    [HasPermission(Permissions.AiContentView)]
    public async Task<ActionResult<IReadOnlyList<AiProviderStatusDto>>> GetProviders(CancellationToken ct)
    {
        var providers = await _aiContentService.GetAvailableProvidersAsync(ct);
        return Ok(providers);
    }
}
