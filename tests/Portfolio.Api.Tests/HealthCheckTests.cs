using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Portfolio.Api.Tests;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("Healthy");
    }

    private sealed class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public double TotalDuration { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
