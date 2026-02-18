using Portfolio.Application.DTOs.Content;

namespace Portfolio.Application.Interfaces;

public interface ISiteContentService
{
    // Singletons
    Task<SiteSettingsDto> GetSiteSettingsAsync(CancellationToken ct = default);
    Task<SiteSettingsDto> UpdateSiteSettingsAsync(SiteSettingsUpdateDto dto, CancellationToken ct = default);
    Task<HeroSectionDto> GetHeroSectionAsync(CancellationToken ct = default);
    Task<HeroSectionDto> UpdateHeroSectionAsync(HeroSectionUpdateDto dto, CancellationToken ct = default);
    Task<AboutSectionDto> GetAboutSectionAsync(CancellationToken ct = default);
    Task<AboutSectionDto> UpdateAboutSectionAsync(AboutSectionUpdateDto dto, CancellationToken ct = default);

    // Skills
    Task<IReadOnlyList<SkillDto>> GetAllSkillsAsync(CancellationToken ct = default);
    Task<SkillDto> CreateSkillAsync(SkillCreateDto dto, CancellationToken ct = default);
    Task<SkillDto> UpdateSkillAsync(Guid id, SkillUpdateDto dto, CancellationToken ct = default);
    Task DeleteSkillAsync(Guid id, CancellationToken ct = default);

    // Experiences
    Task<IReadOnlyList<ExperienceDto>> GetAllExperiencesAsync(CancellationToken ct = default);
    Task<ExperienceDto> CreateExperienceAsync(ExperienceCreateDto dto, CancellationToken ct = default);
    Task<ExperienceDto> UpdateExperienceAsync(Guid id, ExperienceUpdateDto dto, CancellationToken ct = default);
    Task DeleteExperienceAsync(Guid id, CancellationToken ct = default);

    // Services
    Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default);
    Task<ServiceDto> CreateServiceAsync(ServiceCreateDto dto, CancellationToken ct = default);
    Task<ServiceDto> UpdateServiceAsync(Guid id, ServiceUpdateDto dto, CancellationToken ct = default);
    Task DeleteServiceAsync(Guid id, CancellationToken ct = default);

    // Testimonials
    Task<IReadOnlyList<TestimonialDto>> GetAllTestimonialsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TestimonialDto>> GetPublishedTestimonialsAsync(CancellationToken ct = default);
    Task<TestimonialDto> CreateTestimonialAsync(TestimonialCreateDto dto, CancellationToken ct = default);
    Task<TestimonialDto> UpdateTestimonialAsync(Guid id, TestimonialUpdateDto dto, CancellationToken ct = default);
    Task DeleteTestimonialAsync(Guid id, CancellationToken ct = default);

    // Social Links
    Task<IReadOnlyList<SocialLinkDto>> GetAllSocialLinksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SocialLinkDto>> GetVisibleSocialLinksAsync(CancellationToken ct = default);
    Task<SocialLinkDto> CreateSocialLinkAsync(SocialLinkCreateDto dto, CancellationToken ct = default);
    Task<SocialLinkDto> UpdateSocialLinkAsync(Guid id, SocialLinkUpdateDto dto, CancellationToken ct = default);
    Task DeleteSocialLinkAsync(Guid id, CancellationToken ct = default);

    // Menu Items
    Task<IReadOnlyList<MenuItemDto>> GetAllMenuItemsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MenuItemDto>> GetVisibleMenuItemsAsync(CancellationToken ct = default);
    Task<MenuItemDto> CreateMenuItemAsync(MenuItemCreateDto dto, CancellationToken ct = default);
    Task<MenuItemDto> UpdateMenuItemAsync(Guid id, MenuItemUpdateDto dto, CancellationToken ct = default);
    Task DeleteMenuItemAsync(Guid id, CancellationToken ct = default);
}
