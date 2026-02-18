using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Portfolio.Application.DTOs.Auth;
using Xunit;

namespace Portfolio.Api.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── Login Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAccessToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@portfolio.dev",
            Password = "Admin@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
        authResponse.RefreshToken.Should().NotBeNullOrWhiteSpace();
        authResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        authResponse.RequiresTwoFactor.Should().BeFalse();
        authResponse.User.Should().NotBeNull();
        authResponse.User!.Email.Should().Be("admin@portfolio.dev");
        authResponse.User.FullName.Should().Be("Super Admin");
        authResponse.User.Roles.Should().Contain("SuperAdmin");
        authResponse.User.Permissions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsForbidden()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@portfolio.dev",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        // The AuthService throws UnauthorizedAccessException which the
        // GlobalExceptionHandlerMiddleware maps to 403 Forbidden.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsForbidden()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        // The AuthService throws UnauthorizedAccessException for unknown users,
        // which the GlobalExceptionHandlerMiddleware maps to 403 Forbidden.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Refresh Token Tests ──────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewAccessToken()
    {
        // Arrange - First login to get tokens
        var loginDto = new LoginDto
        {
            Email = "admin@portfolio.dev",
            Password = "Admin@123456"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
        authResponse.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var refreshDto = new RefreshTokenDto
        {
            AccessToken = authResponse.AccessToken!,
            RefreshToken = authResponse.RefreshToken!
        };

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshDto);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
        newAuthResponse.RefreshToken.Should().NotBeNullOrWhiteSpace();
        newAuthResponse.User.Should().NotBeNull();
        newAuthResponse.User!.Email.Should().Be("admin@portfolio.dev");

        // The new token should be different from the original
        newAuthResponse.AccessToken.Should().NotBe(authResponse.AccessToken);
    }

    // ── Protected Endpoint Tests ─────────────────────────────────────

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        // The [Authorize] attribute causes JWT middleware to return 401
        // when no token is provided.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsOkWithUserInfo()
    {
        // Arrange
        var token = await _factory.GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        content.Should().NotBeNull();
        content!.Email.Should().Be("admin@portfolio.dev");
        content.Roles.Should().Contain("SuperAdmin");
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this.is.not.a.valid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// DTO to deserialize the /auth/me response.
    /// </summary>
    private sealed class CurrentUserResponse
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string[] Roles { get; set; } = [];
        public string[] Permissions { get; set; } = [];
        public bool IsTwoFactorEnabled { get; set; }
    }
}
