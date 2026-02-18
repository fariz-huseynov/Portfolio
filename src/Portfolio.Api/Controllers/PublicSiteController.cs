using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.DTOs.Content;
using Asp.Versioning;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/site")]
[AllowAnonymous]
[EnableRateLimiting("PublicApi")]
public class PublicSiteController : ControllerBase
{
    private readonly ISiteContentService _siteContentService;

    public PublicSiteController(ISiteContentService siteContentService)
    {
        _siteContentService = siteContentService;
    }

    [HttpGet("settings")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<SiteSettingsDto>> GetSettings(CancellationToken ct)
    {
        var settings = await _siteContentService.GetSiteSettingsAsync(ct);
        return Ok(settings);
    }

    [HttpGet("hero")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<HeroSectionDto>> GetHero(CancellationToken ct)
    {
        var hero = await _siteContentService.GetHeroSectionAsync(ct);
        return Ok(hero);
    }

    [HttpGet("about")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<AboutSectionDto>> GetAbout(CancellationToken ct)
    {
        var about = await _siteContentService.GetAboutSectionAsync(ct);
        return Ok(about);
    }

    [HttpGet("skills")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> GetSkills(CancellationToken ct)
    {
        var skills = await _siteContentService.GetAllSkillsAsync(ct);
        return Ok(skills);
    }

    [HttpGet("experiences")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<ExperienceDto>>> GetExperiences(CancellationToken ct)
    {
        var experiences = await _siteContentService.GetAllExperiencesAsync(ct);
        return Ok(experiences);
    }

    [HttpGet("services")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetServices(CancellationToken ct)
    {
        var services = await _siteContentService.GetAllServicesAsync(ct);
        return Ok(services);
    }

    [HttpGet("testimonials")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<TestimonialDto>>> GetTestimonials(CancellationToken ct)
    {
        var testimonials = await _siteContentService.GetPublishedTestimonialsAsync(ct);
        return Ok(testimonials);
    }

    [HttpGet("social-links")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<SocialLinkDto>>> GetSocialLinks(CancellationToken ct)
    {
        var links = await _siteContentService.GetVisibleSocialLinksAsync(ct);
        return Ok(links);
    }

    [HttpGet("menu")]
    [OutputCache(PolicyName = "PublicContent")]
    public async Task<ActionResult<IReadOnlyList<MenuItemDto>>> GetMenu(CancellationToken ct)
    {
        var items = await _siteContentService.GetVisibleMenuItemsAsync(ct);
        return Ok(items);
    }
}
