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
[Route("api/v{version:apiVersion}/admin/content")]
public class AdminContentController : ControllerBase
{
    private readonly ISiteContentService _siteContentService;
    private readonly IOutputCacheStore _outputCacheStore;

    public AdminContentController(
        ISiteContentService siteContentService,
        IOutputCacheStore outputCacheStore)
    {
        _siteContentService = siteContentService;
        _outputCacheStore = outputCacheStore;
    }

    // ── Hero Section ───────────────────────────────────────────────────

    [HttpGet("hero")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<HeroSectionDto>> GetHero(CancellationToken ct)
    {
        var hero = await _siteContentService.GetHeroSectionAsync(ct);
        return Ok(hero);
    }

    [HttpPut("hero")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<HeroSectionDto>> UpdateHero(
        [FromBody] HeroSectionUpdateDto dto, CancellationToken ct)
    {
        var hero = await _siteContentService.UpdateHeroSectionAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(hero);
    }

    // ── About Section ──────────────────────────────────────────────────

    [HttpGet("about")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<AboutSectionDto>> GetAbout(CancellationToken ct)
    {
        var about = await _siteContentService.GetAboutSectionAsync(ct);
        return Ok(about);
    }

    [HttpPut("about")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<AboutSectionDto>> UpdateAbout(
        [FromBody] AboutSectionUpdateDto dto, CancellationToken ct)
    {
        var about = await _siteContentService.UpdateAboutSectionAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(about);
    }

    // ── Skills ─────────────────────────────────────────────────────────

    [HttpGet("skills")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> GetSkills(CancellationToken ct)
    {
        var skills = await _siteContentService.GetAllSkillsAsync(ct);
        return Ok(skills);
    }

    [HttpPost("skills")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<SkillDto>> CreateSkill(
        [FromBody] SkillCreateDto dto, CancellationToken ct)
    {
        var skill = await _siteContentService.CreateSkillAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Created($"api/v1/admin/content/skills/{skill.Id}", skill);
    }

    [HttpPut("skills/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<SkillDto>> UpdateSkill(
        Guid id, [FromBody] SkillUpdateDto dto, CancellationToken ct)
    {
        var skill = await _siteContentService.UpdateSkillAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(skill);
    }

    [HttpDelete("skills/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<IActionResult> DeleteSkill(Guid id, CancellationToken ct)
    {
        await _siteContentService.DeleteSkillAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    // ── Experiences ────────────────────────────────────────────────────

    [HttpGet("experiences")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<IReadOnlyList<ExperienceDto>>> GetExperiences(CancellationToken ct)
    {
        var experiences = await _siteContentService.GetAllExperiencesAsync(ct);
        return Ok(experiences);
    }

    [HttpPost("experiences")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<ExperienceDto>> CreateExperience(
        [FromBody] ExperienceCreateDto dto, CancellationToken ct)
    {
        var experience = await _siteContentService.CreateExperienceAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Created($"api/v1/admin/content/experiences/{experience.Id}", experience);
    }

    [HttpPut("experiences/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<ExperienceDto>> UpdateExperience(
        Guid id, [FromBody] ExperienceUpdateDto dto, CancellationToken ct)
    {
        var experience = await _siteContentService.UpdateExperienceAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(experience);
    }

    [HttpDelete("experiences/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<IActionResult> DeleteExperience(Guid id, CancellationToken ct)
    {
        await _siteContentService.DeleteExperienceAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    // ── Services ───────────────────────────────────────────────────────

    [HttpGet("services")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetServices(CancellationToken ct)
    {
        var services = await _siteContentService.GetAllServicesAsync(ct);
        return Ok(services);
    }

    [HttpPost("services")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<ServiceDto>> CreateService(
        [FromBody] ServiceCreateDto dto, CancellationToken ct)
    {
        var service = await _siteContentService.CreateServiceAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Created($"api/v1/admin/content/services/{service.Id}", service);
    }

    [HttpPut("services/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<ServiceDto>> UpdateService(
        Guid id, [FromBody] ServiceUpdateDto dto, CancellationToken ct)
    {
        var service = await _siteContentService.UpdateServiceAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(service);
    }

    [HttpDelete("services/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<IActionResult> DeleteService(Guid id, CancellationToken ct)
    {
        await _siteContentService.DeleteServiceAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    // ── Testimonials ───────────────────────────────────────────────────

    [HttpGet("testimonials")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<IReadOnlyList<TestimonialDto>>> GetTestimonials(CancellationToken ct)
    {
        var testimonials = await _siteContentService.GetAllTestimonialsAsync(ct);
        return Ok(testimonials);
    }

    [HttpPost("testimonials")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<TestimonialDto>> CreateTestimonial(
        [FromBody] TestimonialCreateDto dto, CancellationToken ct)
    {
        var testimonial = await _siteContentService.CreateTestimonialAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Created($"api/v1/admin/content/testimonials/{testimonial.Id}", testimonial);
    }

    [HttpPut("testimonials/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<TestimonialDto>> UpdateTestimonial(
        Guid id, [FromBody] TestimonialUpdateDto dto, CancellationToken ct)
    {
        var testimonial = await _siteContentService.UpdateTestimonialAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(testimonial);
    }

    [HttpDelete("testimonials/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<IActionResult> DeleteTestimonial(Guid id, CancellationToken ct)
    {
        await _siteContentService.DeleteTestimonialAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    // ── Social Links ───────────────────────────────────────────────────

    [HttpGet("social-links")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<IReadOnlyList<SocialLinkDto>>> GetSocialLinks(CancellationToken ct)
    {
        var links = await _siteContentService.GetAllSocialLinksAsync(ct);
        return Ok(links);
    }

    [HttpPost("social-links")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<SocialLinkDto>> CreateSocialLink(
        [FromBody] SocialLinkCreateDto dto, CancellationToken ct)
    {
        var link = await _siteContentService.CreateSocialLinkAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Created($"api/v1/admin/content/social-links/{link.Id}", link);
    }

    [HttpPut("social-links/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<SocialLinkDto>> UpdateSocialLink(
        Guid id, [FromBody] SocialLinkUpdateDto dto, CancellationToken ct)
    {
        var link = await _siteContentService.UpdateSocialLinkAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(link);
    }

    [HttpDelete("social-links/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<IActionResult> DeleteSocialLink(Guid id, CancellationToken ct)
    {
        await _siteContentService.DeleteSocialLinkAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }

    // ── Menu Items ─────────────────────────────────────────────────────

    [HttpGet("menu-items")]
    [HasPermission(Permissions.SiteContentView)]
    public async Task<ActionResult<IReadOnlyList<MenuItemDto>>> GetMenuItems(CancellationToken ct)
    {
        var items = await _siteContentService.GetAllMenuItemsAsync(ct);
        return Ok(items);
    }

    [HttpPost("menu-items")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<MenuItemDto>> CreateMenuItem(
        [FromBody] MenuItemCreateDto dto, CancellationToken ct)
    {
        var item = await _siteContentService.CreateMenuItemAsync(dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Created($"api/v1/admin/content/menu-items/{item.Id}", item);
    }

    [HttpPut("menu-items/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<ActionResult<MenuItemDto>> UpdateMenuItem(
        Guid id, [FromBody] MenuItemUpdateDto dto, CancellationToken ct)
    {
        var item = await _siteContentService.UpdateMenuItemAsync(id, dto, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return Ok(item);
    }

    [HttpDelete("menu-items/{id:guid}")]
    [HasPermission(Permissions.SiteContentEdit)]
    public async Task<IActionResult> DeleteMenuItem(Guid id, CancellationToken ct)
    {
        await _siteContentService.DeleteMenuItemAsync(id, ct);
        await _outputCacheStore.EvictByTagAsync(PolicyNames.PublicContentTag, ct);
        return NoContent();
    }
}
