using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs.Content;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class SiteContentService : ISiteContentService
{
    private readonly IRepository<SiteSettings> _siteSettingsRepo;
    private readonly IRepository<HeroSection> _heroRepo;
    private readonly IRepository<AboutSection> _aboutRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IRepository<Experience> _experienceRepo;
    private readonly IRepository<Service> _serviceRepo;
    private readonly IRepository<Testimonial> _testimonialRepo;
    private readonly IRepository<SocialLink> _socialLinkRepo;
    private readonly IRepository<MenuItem> _menuItemRepo;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IHybridCacheService _cache;
    private readonly TimeSpan _cacheDuration;

    public SiteContentService(
        IRepository<SiteSettings> siteSettingsRepo,
        IRepository<HeroSection> heroRepo,
        IRepository<AboutSection> aboutRepo,
        IRepository<Skill> skillRepo,
        IRepository<Experience> experienceRepo,
        IRepository<Service> serviceRepo,
        IRepository<Testimonial> testimonialRepo,
        IRepository<SocialLink> socialLinkRepo,
        IRepository<MenuItem> menuItemRepo,
        IHtmlSanitizerService htmlSanitizer,
        IHybridCacheService cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _siteSettingsRepo = siteSettingsRepo;
        _heroRepo = heroRepo;
        _aboutRepo = aboutRepo;
        _skillRepo = skillRepo;
        _experienceRepo = experienceRepo;
        _serviceRepo = serviceRepo;
        _testimonialRepo = testimonialRepo;
        _socialLinkRepo = socialLinkRepo;
        _menuItemRepo = menuItemRepo;
        _htmlSanitizer = htmlSanitizer;
        _cache = cache;
        _cacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.SiteContentMinutes);
    }

    // ── Site Settings (Singleton) ──────────────────────────────────────

    public async Task<SiteSettingsDto> GetSiteSettingsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.SiteSettings,
            async () =>
            {
                var all = await _siteSettingsRepo.GetAllAsync(ct);
                var entity = all.FirstOrDefault()
                    ?? throw new KeyNotFoundException("Site settings not found.");
                return MapSiteSettings(entity);
            },
            _cacheDuration,
            null,
            ct
        ) ?? throw new KeyNotFoundException("Site settings not found.");
    }

    public async Task<SiteSettingsDto> UpdateSiteSettingsAsync(SiteSettingsUpdateDto dto, CancellationToken ct = default)
    {
        var all = await _siteSettingsRepo.GetAllAsync(ct);
        var entity = all.FirstOrDefault()
            ?? throw new KeyNotFoundException("Site settings not found.");

        entity.SiteName = dto.SiteName;
        entity.LogoUrl = dto.LogoUrl;
        entity.FaviconUrl = dto.FaviconUrl;
        entity.SeoTitle = dto.SeoTitle;
        entity.SeoDescription = dto.SeoDescription;
        entity.SeoKeywords = dto.SeoKeywords;
        entity.GoogleAnalyticsId = dto.GoogleAnalyticsId;
        entity.FooterText = dto.FooterText;
        entity.UpdatedAt = DateTime.UtcNow;

        await _siteSettingsRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.SiteSettings, ct);
        return MapSiteSettings(entity);
    }

    // ── Hero Section (Singleton) ───────────────────────────────────────

    public async Task<HeroSectionDto> GetHeroSectionAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.HeroSection,
            async () =>
            {
                var all = await _heroRepo.GetAllAsync(ct);
                var entity = all.FirstOrDefault()
                    ?? throw new KeyNotFoundException("Hero section not found.");
                return MapHero(entity);
            },
            _cacheDuration,
            null,
            ct
        ) ?? throw new KeyNotFoundException("Hero section not found.");
    }

    public async Task<HeroSectionDto> UpdateHeroSectionAsync(HeroSectionUpdateDto dto, CancellationToken ct = default)
    {
        var all = await _heroRepo.GetAllAsync(ct);
        var entity = all.FirstOrDefault()
            ?? throw new KeyNotFoundException("Hero section not found.");

        entity.Title = dto.Title;
        entity.Subtitle = dto.Subtitle is not null ? _htmlSanitizer.Sanitize(dto.Subtitle) : null;
        entity.BackgroundImageUrl = dto.BackgroundImageUrl;
        entity.CtaText = dto.CtaText;
        entity.CtaUrl = dto.CtaUrl;
        entity.UpdatedAt = DateTime.UtcNow;

        await _heroRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.HeroSection, ct);
        return MapHero(entity);
    }

    // ── About Section (Singleton) ──────────────────────────────────────

    public async Task<AboutSectionDto> GetAboutSectionAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.AboutSection,
            async () =>
            {
                var all = await _aboutRepo.GetAllAsync(ct);
                var entity = all.FirstOrDefault()
                    ?? throw new KeyNotFoundException("About section not found.");
                return MapAbout(entity);
            },
            _cacheDuration,
            null,
            ct
        ) ?? throw new KeyNotFoundException("About section not found.");
    }

    public async Task<AboutSectionDto> UpdateAboutSectionAsync(AboutSectionUpdateDto dto, CancellationToken ct = default)
    {
        var all = await _aboutRepo.GetAllAsync(ct);
        var entity = all.FirstOrDefault()
            ?? throw new KeyNotFoundException("About section not found.");

        entity.Title = dto.Title;
        entity.Content = _htmlSanitizer.Sanitize(dto.Content);
        entity.ProfileImageUrl = dto.ProfileImageUrl;
        entity.ResumeUrl = dto.ResumeUrl;
        entity.UpdatedAt = DateTime.UtcNow;

        await _aboutRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.AboutSection, ct);
        return MapAbout(entity);
    }

    // ── Skills ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SkillDto>> GetAllSkillsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Skills,
            async () =>
            {
                var items = await _skillRepo.GetAllAsync(ct);
                return (IReadOnlyList<SkillDto>)items.OrderBy(x => x.SortOrder).Select(MapSkill).ToList();
            },
            _cacheDuration,
            null,
            ct
        ) ?? new List<SkillDto>();
    }

    public async Task<SkillDto> CreateSkillAsync(SkillCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Skill
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Category = dto.Category,
            Proficiency = dto.Proficiency,
            IconUrl = dto.IconUrl,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };
        await _skillRepo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Skills, ct);
        return MapSkill(entity);
    }

    public async Task<SkillDto> UpdateSkillAsync(Guid id, SkillUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _skillRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Skill with ID {id} not found.");

        entity.Name = dto.Name;
        entity.Category = dto.Category;
        entity.Proficiency = dto.Proficiency;
        entity.IconUrl = dto.IconUrl;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _skillRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Skills, ct);
        return MapSkill(entity);
    }

    public async Task DeleteSkillAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _skillRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Skill with ID {id} not found.");
        await _skillRepo.DeleteAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Skills, ct);
    }

    // ── Experiences ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ExperienceDto>> GetAllExperiencesAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Experiences,
            async () =>
            {
                var items = await _experienceRepo.GetAllAsync(ct);
                return (IReadOnlyList<ExperienceDto>)items.OrderBy(x => x.SortOrder).Select(MapExperience).ToList();
            },
            _cacheDuration,
            null,
            ct
        ) ?? new List<ExperienceDto>();
    }

    public async Task<ExperienceDto> CreateExperienceAsync(ExperienceCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Experience
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Company = dto.Company,
            Location = dto.Location,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = _htmlSanitizer.Sanitize(dto.Description),
            IsCurrent = dto.IsCurrent,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };
        await _experienceRepo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Experiences, ct);
        return MapExperience(entity);
    }

    public async Task<ExperienceDto> UpdateExperienceAsync(Guid id, ExperienceUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _experienceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Experience with ID {id} not found.");

        entity.Title = dto.Title;
        entity.Company = dto.Company;
        entity.Location = dto.Location;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Description = _htmlSanitizer.Sanitize(dto.Description);
        entity.IsCurrent = dto.IsCurrent;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _experienceRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Experiences, ct);
        return MapExperience(entity);
    }

    public async Task DeleteExperienceAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _experienceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Experience with ID {id} not found.");
        await _experienceRepo.DeleteAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Experiences, ct);
    }

    // ── Services ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Services,
            async () =>
            {
                var items = await _serviceRepo.GetAllAsync(ct);
                return (IReadOnlyList<ServiceDto>)items.OrderBy(x => x.SortOrder).Select(MapService).ToList();
            },
            _cacheDuration,
            null,
            ct
        ) ?? new List<ServiceDto>();
    }

    public async Task<ServiceDto> CreateServiceAsync(ServiceCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Service
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = _htmlSanitizer.Sanitize(dto.Description),
            IconUrl = dto.IconUrl,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };
        await _serviceRepo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Services, ct);
        return MapService(entity);
    }

    public async Task<ServiceDto> UpdateServiceAsync(Guid id, ServiceUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _serviceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Service with ID {id} not found.");

        entity.Title = dto.Title;
        entity.Description = _htmlSanitizer.Sanitize(dto.Description);
        entity.IconUrl = dto.IconUrl;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _serviceRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Services, ct);
        return MapService(entity);
    }

    public async Task DeleteServiceAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _serviceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Service with ID {id} not found.");
        await _serviceRepo.DeleteAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Services, ct);
    }

    // ── Testimonials ───────────────────────────────────────────────────

    public async Task<IReadOnlyList<TestimonialDto>> GetAllTestimonialsAsync(CancellationToken ct = default)
    {
        var items = await _testimonialRepo.GetAllAsync(ct);
        return items.OrderBy(x => x.SortOrder).Select(MapTestimonial).ToList();
    }

    public async Task<IReadOnlyList<TestimonialDto>> GetPublishedTestimonialsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Testimonials,
            async () =>
            {
                var items = await _testimonialRepo.FindAsync(x => x.IsPublished, ct);
                return (IReadOnlyList<TestimonialDto>)items.OrderBy(x => x.SortOrder).Select(MapTestimonial).ToList();
            },
            _cacheDuration,
            null,
            ct
        ) ?? new List<TestimonialDto>();
    }

    public async Task<TestimonialDto> CreateTestimonialAsync(TestimonialCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Testimonial
        {
            Id = Guid.NewGuid(),
            AuthorName = dto.AuthorName,
            AuthorTitle = dto.AuthorTitle,
            AuthorCompany = dto.AuthorCompany,
            AuthorImageUrl = dto.AuthorImageUrl,
            Quote = dto.Quote,
            Rating = dto.Rating,
            IsPublished = dto.IsPublished,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };
        await _testimonialRepo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Testimonials, ct);
        return MapTestimonial(entity);
    }

    public async Task<TestimonialDto> UpdateTestimonialAsync(Guid id, TestimonialUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _testimonialRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Testimonial with ID {id} not found.");

        entity.AuthorName = dto.AuthorName;
        entity.AuthorTitle = dto.AuthorTitle;
        entity.AuthorCompany = dto.AuthorCompany;
        entity.AuthorImageUrl = dto.AuthorImageUrl;
        entity.Quote = dto.Quote;
        entity.Rating = dto.Rating;
        entity.IsPublished = dto.IsPublished;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _testimonialRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Testimonials, ct);
        return MapTestimonial(entity);
    }

    public async Task DeleteTestimonialAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _testimonialRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Testimonial with ID {id} not found.");
        await _testimonialRepo.DeleteAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.Testimonials, ct);
    }

    // ── Social Links ───────────────────────────────────────────────────

    public async Task<IReadOnlyList<SocialLinkDto>> GetAllSocialLinksAsync(CancellationToken ct = default)
    {
        var items = await _socialLinkRepo.GetAllAsync(ct);
        return items.OrderBy(x => x.SortOrder).Select(MapSocialLink).ToList();
    }

    public async Task<IReadOnlyList<SocialLinkDto>> GetVisibleSocialLinksAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.SocialLinks,
            async () =>
            {
                var items = await _socialLinkRepo.FindAsync(x => x.IsVisible, ct);
                return (IReadOnlyList<SocialLinkDto>)items.OrderBy(x => x.SortOrder).Select(MapSocialLink).ToList();
            },
            _cacheDuration,
            null,
            ct
        ) ?? new List<SocialLinkDto>();
    }

    public async Task<SocialLinkDto> CreateSocialLinkAsync(SocialLinkCreateDto dto, CancellationToken ct = default)
    {
        var entity = new SocialLink
        {
            Id = Guid.NewGuid(),
            Platform = dto.Platform,
            Url = dto.Url,
            IconUrl = dto.IconUrl,
            SortOrder = dto.SortOrder,
            IsVisible = dto.IsVisible,
            CreatedAt = DateTime.UtcNow
        };
        await _socialLinkRepo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.SocialLinks, ct);
        return MapSocialLink(entity);
    }

    public async Task<SocialLinkDto> UpdateSocialLinkAsync(Guid id, SocialLinkUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _socialLinkRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Social link with ID {id} not found.");

        entity.Platform = dto.Platform;
        entity.Url = dto.Url;
        entity.IconUrl = dto.IconUrl;
        entity.SortOrder = dto.SortOrder;
        entity.IsVisible = dto.IsVisible;
        entity.UpdatedAt = DateTime.UtcNow;

        await _socialLinkRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.SocialLinks, ct);
        return MapSocialLink(entity);
    }

    public async Task DeleteSocialLinkAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _socialLinkRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Social link with ID {id} not found.");
        await _socialLinkRepo.DeleteAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.SocialLinks, ct);
    }

    // ── Menu Items ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<MenuItemDto>> GetAllMenuItemsAsync(CancellationToken ct = default)
    {
        var items = await _menuItemRepo.GetAllAsync(ct);
        return BuildMenuTree(items);
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetVisibleMenuItemsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.MenuItems,
            async () =>
            {
                var items = await _menuItemRepo.FindAsync(x => x.IsVisible, ct);
                return (IReadOnlyList<MenuItemDto>)BuildMenuTree(items);
            },
            _cacheDuration,
            null,
            ct
        ) ?? new List<MenuItemDto>();
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(MenuItemCreateDto dto, CancellationToken ct = default)
    {
        var entity = new MenuItem
        {
            Id = Guid.NewGuid(),
            Label = dto.Label,
            Url = dto.Url,
            SortOrder = dto.SortOrder,
            IsVisible = dto.IsVisible,
            ParentId = dto.ParentId,
            CreatedAt = DateTime.UtcNow
        };
        await _menuItemRepo.AddAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.MenuItems, ct);
        return MapMenuItem(entity);
    }

    public async Task<MenuItemDto> UpdateMenuItemAsync(Guid id, MenuItemUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _menuItemRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Menu item with ID {id} not found.");

        entity.Label = dto.Label;
        entity.Url = dto.Url;
        entity.SortOrder = dto.SortOrder;
        entity.IsVisible = dto.IsVisible;
        entity.ParentId = dto.ParentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _menuItemRepo.UpdateAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.MenuItems, ct);
        return MapMenuItem(entity);
    }

    public async Task DeleteMenuItemAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _menuItemRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Menu item with ID {id} not found.");
        await _menuItemRepo.DeleteAsync(entity, ct);
        await _cache.RemoveAsync(CacheKeys.MenuItems, ct);
    }

    // ── Private Mappers ────────────────────────────────────────────────

    private static SiteSettingsDto MapSiteSettings(SiteSettings e) => new()
    {
        Id = e.Id,
        SiteName = e.SiteName,
        LogoUrl = e.LogoUrl,
        FaviconUrl = e.FaviconUrl,
        SeoTitle = e.SeoTitle,
        SeoDescription = e.SeoDescription,
        SeoKeywords = e.SeoKeywords,
        GoogleAnalyticsId = e.GoogleAnalyticsId,
        FooterText = e.FooterText
    };

    private static HeroSectionDto MapHero(HeroSection e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Subtitle = e.Subtitle,
        BackgroundImageUrl = e.BackgroundImageUrl,
        CtaText = e.CtaText,
        CtaUrl = e.CtaUrl
    };

    private static AboutSectionDto MapAbout(AboutSection e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Content = e.Content,
        ProfileImageUrl = e.ProfileImageUrl,
        ResumeUrl = e.ResumeUrl
    };

    private static SkillDto MapSkill(Skill e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Category = e.Category,
        Proficiency = e.Proficiency,
        IconUrl = e.IconUrl,
        SortOrder = e.SortOrder
    };

    private static ExperienceDto MapExperience(Experience e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Company = e.Company,
        Location = e.Location,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Description = e.Description,
        IsCurrent = e.IsCurrent,
        SortOrder = e.SortOrder
    };

    private static ServiceDto MapService(Service e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        IconUrl = e.IconUrl,
        SortOrder = e.SortOrder
    };

    private static TestimonialDto MapTestimonial(Testimonial e) => new()
    {
        Id = e.Id,
        AuthorName = e.AuthorName,
        AuthorTitle = e.AuthorTitle,
        AuthorCompany = e.AuthorCompany,
        AuthorImageUrl = e.AuthorImageUrl,
        Quote = e.Quote,
        Rating = e.Rating,
        IsPublished = e.IsPublished,
        SortOrder = e.SortOrder
    };

    private static SocialLinkDto MapSocialLink(SocialLink e) => new()
    {
        Id = e.Id,
        Platform = e.Platform,
        Url = e.Url,
        IconUrl = e.IconUrl,
        SortOrder = e.SortOrder,
        IsVisible = e.IsVisible
    };

    private static MenuItemDto MapMenuItem(MenuItem e) => new()
    {
        Id = e.Id,
        Label = e.Label,
        Url = e.Url,
        SortOrder = e.SortOrder,
        IsVisible = e.IsVisible,
        ParentId = e.ParentId
    };

    private static List<MenuItemDto> BuildMenuTree(IReadOnlyList<MenuItem> items)
    {
        var lookup = items.ToLookup(x => x.ParentId);
        return BuildChildren(null);

        List<MenuItemDto> BuildChildren(Guid? parentId)
        {
            return lookup[parentId]
                .OrderBy(x => x.SortOrder)
                .Select(e => new MenuItemDto
                {
                    Id = e.Id,
                    Label = e.Label,
                    Url = e.Url,
                    SortOrder = e.SortOrder,
                    IsVisible = e.IsVisible,
                    ParentId = e.ParentId,
                    Children = BuildChildren(e.Id)
                })
                .ToList();
        }
    }
}
