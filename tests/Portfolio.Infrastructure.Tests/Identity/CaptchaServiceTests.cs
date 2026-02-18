using FluentAssertions;
using Lazy.Captcha.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using CaptchaService = Portfolio.Infrastructure.Identity.CaptchaService;

namespace Portfolio.Infrastructure.Tests.Identity;

public class CaptchaServiceTests
{
    private readonly ICaptcha _captcha;
    private readonly ILogger<CaptchaService> _logger;
    private readonly CaptchaService _sut;

    public CaptchaServiceTests()
    {
        _captcha = Substitute.For<ICaptcha>();
        _logger = Substitute.For<ILogger<CaptchaService>>();
        _sut = new CaptchaService(_captcha, _logger);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateTokenAsync_ReturnsFalse_ForNullOrEmptyToken(string? token)
    {
        // Act
        var result = await _sut.ValidateTokenAsync(token!, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _captcha.DidNotReceive().Validate(Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData("no-colon-here")]
    [InlineData("justanid")]
    [InlineData("abc")]
    public async Task ValidateTokenAsync_ReturnsFalse_ForInvalidFormat_NoColon(string token)
    {
        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeFalse();
        _captcha.DidNotReceive().Validate(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsTrue_WhenCaptchaValidates()
    {
        // Arrange
        const string id = "captcha-id-123";
        const string code = "AB12";
        var token = $"{id}:{code}";

        _captcha.Validate(id, code).Returns(true);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeTrue();
        _captcha.Received(1).Validate(id, code);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalse_WhenCaptchaRejects()
    {
        // Arrange
        const string id = "captcha-id-456";
        const string code = "WRONG";
        var token = $"{id}:{code}";

        _captcha.Validate(id, code).Returns(false);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeFalse();
        _captcha.Received(1).Validate(id, code);
    }

    [Fact]
    public async Task ValidateTokenAsync_HandlesTokenWithMultipleColons()
    {
        // The Split(':', 2) should only split on the first colon,
        // so "id:code:extra" becomes id="id", code="code:extra"
        const string id = "captcha-id";
        const string code = "code:with:colons";
        var token = $"{id}:{code}";

        _captcha.Validate(id, code).Returns(true);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeTrue();
        _captcha.Received(1).Validate(id, code);
    }
}
