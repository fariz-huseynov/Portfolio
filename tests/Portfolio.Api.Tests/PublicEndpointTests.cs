using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Portfolio.Api.Tests;

public class PublicEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PublicEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Site Settings ────────────────────────────────────────────────

    [Fact]
    public async Task GetSiteSettings_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/settings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<SiteSettingsResponse>();
        content.Should().NotBeNull();
        // Verify the seeded site settings
        content!.SiteName.Should().Be("My Portfolio");
    }

    [Fact]
    public async Task GetSiteSettings_DoesNotRequireAuth()
    {
        // Act - No Authorization header set
        var response = await _client.GetAsync("/api/v1/site/settings");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Hero Section ─────────────────────────────────────────────────

    [Fact]
    public async Task GetHeroSection_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/hero");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<HeroSectionResponse>();
        content.Should().NotBeNull();
        // Verify the seeded hero section
        content!.Title.Should().Be("Welcome to My Portfolio");
    }

    [Fact]
    public async Task GetHeroSection_DoesNotRequireAuth()
    {
        // Act - No Authorization header set
        var response = await _client.GetAsync("/api/v1/site/hero");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Captcha ──────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateCaptcha_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/captcha/generate");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<CaptchaResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeNullOrWhiteSpace();
        content.Image.Should().NotBeNullOrWhiteSpace();
        content.Image.Should().StartWith("data:image/png;base64,");
    }

    [Fact]
    public async Task GenerateCaptcha_DoesNotRequireAuth()
    {
        // Act - No Authorization header set
        var response = await _client.GetAsync("/api/v1/captcha/generate");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Other Public Endpoints ───────────────────────────────────────

    [Fact]
    public async Task GetAboutSection_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/about");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSkills_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/skills");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExperiences_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/experiences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServices_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTestimonials_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/testimonials");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSocialLinks_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/social-links");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMenu_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/site/menu");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Response DTOs ────────────────────────────────────────────────

    private sealed class SiteSettingsResponse
    {
        public string? SiteName { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? FooterText { get; set; }
    }

    private sealed class HeroSectionResponse
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? CtaText { get; set; }
        public string? CtaUrl { get; set; }
    }

    private sealed class CaptchaResponse
    {
        public string? Id { get; set; }
        public string? Image { get; set; }
    }
}
