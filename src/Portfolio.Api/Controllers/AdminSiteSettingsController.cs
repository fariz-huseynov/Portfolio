using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Portfolio.Api.Authorization;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs.Content;
using Portfolio.Application.Interfaces;
using Asp.Versioning;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/settings")]
public class AdminSiteSettingsController : ControllerBase
{
    private readonly ISiteContentService _siteContentService;
    private readonly IOutputCacheStore _outputCacheStore;

    public AdminSiteSettingsController(
        ISiteContentService siteContentService,
        IOutputCacheStore outputCacheStore)
    {
        _siteContentService = siteContentService;
        _outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [HasPermission(Permissions.SettingsView)]
    public async Task<ActionResult<SiteSettingsDto>> Get(CancellationToken ct)
    {
        var settings = await _siteContentService.GetSiteSettingsAsync(ct);
        return Ok(settings);
    }

    [HttpPut]
    [HasPermission(Permissions.SettingsEdit)]
    public async Task<ActionResult<SiteSettingsDto>> Update(
        [FromBody] SiteSettingsUpdateDto dto, CancellationToken ct)
    {
        var settings = await _siteContentService.UpdateSiteSettingsAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(settings);
    }
}
