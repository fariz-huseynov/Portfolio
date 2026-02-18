#pragma warning disable CS8620 // Nullability mismatch in NSubstitute generic type inference

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs.AiContent;
using Portfolio.Application.Interfaces;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;
using Xunit;

namespace Portfolio.Application.Tests.Services;

public class AiContentServiceTests
{
    private readonly IAiProvider _openAiProvider;
    private readonly IAiGenerationRecordRepository _recordRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly AiContentService _sut;

    public AiContentServiceTests()
    {
        _openAiProvider = Substitute.For<IAiProvider>();
        _openAiProvider.ProviderName.Returns("OpenAi");
        _openAiProvider.IsConfigured.Returns(true);
        _openAiProvider.DefaultModel.Returns("gpt-4o-mini");
        _openAiProvider.SupportedOperations.Returns(new[] { "GenerateBlogPost", "RewriteText", "GenerateImage" });

        _recordRepository = Substitute.For<IAiGenerationRecordRepository>();
        _fileStorage = Substitute.For<IFileStorageService>();

        var settings = Options.Create(new AiSettings
        {
            DefaultProvider = "OpenAi"
        });

        var logger = Substitute.For<ILogger<AiContentService>>();

        _sut = new AiContentService(
            new[] { _openAiProvider },
            _recordRepository,
            _fileStorage,
            settings,
            logger);
    }

    #region GenerateTextAsync

    [Fact]
    public async Task GenerateTextAsync_SavesRecordAndReturnsCompleted()
    {
        // Arrange
        _openAiProvider.GenerateTextAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResult("Generated blog post content", "gpt-4o-mini", 100, 200));

        var request = new AiGenerateTextRequestDto
        {
            OperationType = "GenerateBlogPost",
            Prompt = "Write about .NET 10 features"
        };

        // Act
        var result = await _sut.GenerateTextAsync(request, "user-1");

        // Assert
        result.Status.Should().Be("Completed");
        result.ResultContent.Should().Be("Generated blog post content");
        result.ModelName.Should().Be("gpt-4o-mini");
        result.InputTokens.Should().Be(100);
        result.OutputTokens.Should().Be(200);
        result.Provider.Should().Be("OpenAi");
        result.OperationType.Should().Be("GenerateBlogPost");
        result.CompletedAt.Should().NotBeNull();
        result.DurationSeconds.Should().BeGreaterThanOrEqualTo(0);

        await _recordRepository.Received(1).AddAsync(Arg.Any<AiGenerationRecord>(), Arg.Any<CancellationToken>());
        await _recordRepository.Received(1).UpdateAsync(Arg.Any<AiGenerationRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateTextAsync_WhenProviderFails_SetsStatusToFailed()
    {
        // Arrange
        _openAiProvider.GenerateTextAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        var request = new AiGenerateTextRequestDto
        {
            OperationType = "GenerateBlogPost",
            Prompt = "Write about .NET 10"
        };

        // Act
        var result = await _sut.GenerateTextAsync(request, "user-1");

        // Assert
        result.Status.Should().Be("Failed");
        result.ErrorMessage.Should().Contain("API unavailable");
        result.ResultContent.Should().BeNull();
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task GenerateTextAsync_ThrowsArgumentException_WhenProviderNotRegistered()
    {
        // Arrange
        var request = new AiGenerateTextRequestDto
        {
            OperationType = "GenerateBlogPost",
            Prompt = "test",
            PreferredProvider = "NonExistent"
        };

        // Act
        var act = () => _sut.GenerateTextAsync(request, "user-1");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*NonExistent*");
    }

    [Fact]
    public async Task GenerateTextAsync_ThrowsInvalidOperationException_WhenProviderNotConfigured()
    {
        // Arrange
        var unconfiguredProvider = Substitute.For<IAiProvider>();
        unconfiguredProvider.ProviderName.Returns("Anthropic");
        unconfiguredProvider.IsConfigured.Returns(false);

        var settings = Options.Create(new AiSettings { DefaultProvider = "Anthropic" });
        var logger = Substitute.For<ILogger<AiContentService>>();

        var sut = new AiContentService(
            new[] { unconfiguredProvider },
            _recordRepository,
            _fileStorage,
            settings,
            logger);

        var request = new AiGenerateTextRequestDto
        {
            OperationType = "GenerateBlogPost",
            Prompt = "test"
        };

        // Act
        var act = () => sut.GenerateTextAsync(request, "user-1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Anthropic*not configured*");
    }

    [Fact]
    public async Task GenerateTextAsync_IncludesAdditionalContext_WhenProvided()
    {
        // Arrange
        _openAiProvider.GenerateTextAsync(
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("Additional context: focus on performance")),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new AiTextResult("Content with context", "gpt-4o-mini", 100, 200));

        var request = new AiGenerateTextRequestDto
        {
            OperationType = "GenerateBlogPost",
            Prompt = "Write about .NET 10",
            AdditionalContext = "focus on performance"
        };

        // Act
        var result = await _sut.GenerateTextAsync(request, "user-1");

        // Assert
        result.Status.Should().Be("Completed");
        await _openAiProvider.Received(1).GenerateTextAsync(
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("Additional context: focus on performance")),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region RewriteTextAsync

    [Fact]
    public async Task RewriteTextAsync_DelegatesToGenerateTextAsync()
    {
        // Arrange
        _openAiProvider.GenerateTextAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AiTextResult("Rewritten text", "gpt-4o-mini", 50, 100));

        var request = new AiRewriteTextRequestDto
        {
            OriginalText = "This is the original text",
            Instructions = "Make it more professional"
        };

        // Act
        var result = await _sut.RewriteTextAsync(request, "user-1");

        // Assert
        result.Status.Should().Be("Completed");
        result.ResultContent.Should().Be("Rewritten text");
        result.OperationType.Should().Be("RewriteText");
    }

    #endregion

    #region GenerateImageAsync

    [Fact]
    public async Task GenerateImageAsync_SavesImageAndReturnsCompleted()
    {
        // Arrange
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic bytes
        _openAiProvider.GenerateImageAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AiImageResult(imageData, "image/png", "dall-e-3"));

        _fileStorage.SaveFileAsync(
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(("/uploads/2026/02/ai-generated.png", "ai-generated.png"));

        var request = new AiGenerateImageRequestDto
        {
            Prompt = "A futuristic cityscape"
        };

        // Act
        var result = await _sut.GenerateImageAsync(request, "user-1");

        // Assert
        result.Status.Should().Be("Completed");
        result.ResultImageUrl.Should().Be("/uploads/2026/02/ai-generated.png");
        result.ModelName.Should().Be("dall-e-3");
        result.OperationType.Should().Be("GenerateImage");
    }

    [Fact]
    public async Task GenerateImageAsync_WhenProviderFails_SetsStatusToFailed()
    {
        // Arrange
        _openAiProvider.GenerateImageAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Image generation failed"));

        var request = new AiGenerateImageRequestDto
        {
            Prompt = "A test image"
        };

        // Act
        var result = await _sut.GenerateImageAsync(request, "user-1");

        // Assert
        result.Status.Should().Be("Failed");
        result.ErrorMessage.Should().Contain("Image generation failed");
        result.ResultImageUrl.Should().BeNull();
    }

    #endregion

    #region GetAvailableProvidersAsync

    [Fact]
    public async Task GetAvailableProvidersAsync_ReturnsAllRegisteredProviders()
    {
        // Act
        var result = await _sut.GetAvailableProvidersAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Provider.Should().Be("OpenAi");
        result[0].IsConfigured.Should().BeTrue();
        result[0].DefaultModel.Should().Be("gpt-4o-mini");
        result[0].SupportedOperations.Should().Contain("GenerateBlogPost");
    }

    #endregion

    #region GetGenerationByIdAsync

    [Fact]
    public async Task GetGenerationByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = new AiGenerationRecord
        {
            Id = id,
            Provider = AiProvider.OpenAi,
            OperationType = AiOperationType.GenerateBlogPost,
            Status = AiGenerationStatus.Completed,
            Prompt = "test prompt",
            ResultContent = "Generated content",
            ModelName = "gpt-4o-mini",
            RequestedByUserId = "user-1",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _recordRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(record);

        // Act
        var result = await _sut.GetGenerationByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Provider.Should().Be("OpenAi");
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task GetGenerationByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _recordRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AiGenerationRecord?)null);

        // Act
        var result = await _sut.GetGenerationByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
