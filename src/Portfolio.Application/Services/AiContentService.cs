using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs.AiContent;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class AiContentService : IAiContentService
{
    private readonly IReadOnlyDictionary<string, IAiProvider> _providers;
    private readonly IAiGenerationRecordRepository _recordRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly AiSettings _settings;
    private readonly ILogger<AiContentService> _logger;

    public AiContentService(
        IEnumerable<IAiProvider> providers,
        IAiGenerationRecordRepository recordRepository,
        IFileStorageService fileStorage,
        IOptions<AiSettings> settings,
        ILogger<AiContentService> logger)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
        _recordRepository = recordRepository;
        _fileStorage = fileStorage;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiGenerationResultDto> GenerateTextAsync(
        AiGenerateTextRequestDto request, string userId, CancellationToken ct = default)
    {
        var provider = ResolveProvider(request.PreferredProvider);
        var operationType = Enum.Parse<AiOperationType>(request.OperationType, ignoreCase: true);
        var systemPrompt = AiPromptTemplates.GetSystemPrompt(request.OperationType);

        var userPrompt = string.IsNullOrWhiteSpace(request.AdditionalContext)
            ? request.Prompt
            : $"{request.Prompt}\n\nAdditional context: {request.AdditionalContext}";

        var record = new AiGenerationRecord
        {
            Id = Guid.NewGuid(),
            Provider = Enum.Parse<AiProvider>(provider.ProviderName, ignoreCase: true),
            OperationType = operationType,
            Status = AiGenerationStatus.Processing,
            Prompt = request.Prompt,
            SystemPrompt = systemPrompt,
            RequestedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _recordRepository.AddAsync(record, ct);

        try
        {
            var sw = Stopwatch.StartNew();
            var result = await provider.GenerateTextAsync(systemPrompt, userPrompt, request.PreferredModel, ct);
            sw.Stop();

            record.ResultContent = result.Content;
            record.ModelName = result.ModelUsed;
            record.InputTokens = result.InputTokens;
            record.OutputTokens = result.OutputTokens;
            record.DurationSeconds = sw.Elapsed.TotalSeconds;
            record.Status = AiGenerationStatus.Completed;
            record.CompletedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI text generation failed for operation {OperationType} with provider {Provider}",
                request.OperationType, provider.ProviderName);
            record.Status = AiGenerationStatus.Failed;
            record.ErrorMessage = ex.Message;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await _recordRepository.UpdateAsync(record, ct);
        return MapToDto(record);
    }

    public async Task<AiGenerationResultDto> RewriteTextAsync(
        AiRewriteTextRequestDto request, string userId, CancellationToken ct = default)
    {
        var generateRequest = new AiGenerateTextRequestDto
        {
            OperationType = nameof(AiOperationType.RewriteText),
            Prompt = $"Original text:\n\n{request.OriginalText}\n\nInstructions: {request.Instructions}",
            PreferredProvider = request.PreferredProvider,
            PreferredModel = request.PreferredModel
        };
        return await GenerateTextAsync(generateRequest, userId, ct);
    }

    public async Task<AiGenerationResultDto> GenerateImageAsync(
        AiGenerateImageRequestDto request, string userId, CancellationToken ct = default)
    {
        var provider = ResolveProvider(request.PreferredProvider);

        var record = new AiGenerationRecord
        {
            Id = Guid.NewGuid(),
            Provider = Enum.Parse<AiProvider>(provider.ProviderName, ignoreCase: true),
            OperationType = AiOperationType.GenerateImage,
            Status = AiGenerationStatus.Processing,
            Prompt = request.Prompt,
            RequestedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _recordRepository.AddAsync(record, ct);

        try
        {
            var sw = Stopwatch.StartNew();
            var result = await provider.GenerateImageAsync(request.Prompt, request.Size, request.Style, ct);
            sw.Stop();

            using var imageStream = new MemoryStream(result.ImageData);
            var extension = result.ContentType == "image/png" ? ".png" : ".jpg";
            var (relativePath, _) = await _fileStorage.SaveFileAsync(
                imageStream, $"ai-generated{extension}", result.ContentType, ct);

            record.ResultImageUrl = relativePath;
            record.ModelName = result.ModelUsed;
            record.DurationSeconds = sw.Elapsed.TotalSeconds;
            record.Status = AiGenerationStatus.Completed;
            record.CompletedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI image generation failed with provider {Provider}", provider.ProviderName);
            record.Status = AiGenerationStatus.Failed;
            record.ErrorMessage = ex.Message;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await _recordRepository.UpdateAsync(record, ct);
        return MapToDto(record);
    }

    public async Task<AiGenerationResultDto?> GetGenerationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _recordRepository.GetByIdAsync(id, ct);
        return record is not null ? MapToDto(record) : null;
    }

    public async Task<PagedResult<AiGenerationResultDto>> GetGenerationHistoryAsync(
        PaginationParams pagination, CancellationToken ct = default)
    {
        var (items, totalCount) = await _recordRepository.GetPagedAsync(pagination.Page, pagination.PageSize, ct);
        return PagedResult<AiGenerationResultDto>.Create(
            items.Select(MapToDto).ToList(), pagination.Page, pagination.PageSize, totalCount);
    }

    public Task<IReadOnlyList<AiProviderStatusDto>> GetAvailableProvidersAsync(CancellationToken ct = default)
    {
        var statuses = _providers.Values.Select(p => new AiProviderStatusDto
        {
            Provider = p.ProviderName,
            IsConfigured = p.IsConfigured,
            DefaultModel = p.DefaultModel,
            SupportedOperations = p.SupportedOperations
        }).ToList();

        return Task.FromResult<IReadOnlyList<AiProviderStatusDto>>(statuses);
    }

    private IAiProvider ResolveProvider(string? preferredProvider)
    {
        var providerName = preferredProvider ?? _settings.DefaultProvider;

        if (!_providers.TryGetValue(providerName, out var provider))
            throw new ArgumentException($"AI provider '{providerName}' is not registered.");

        if (!provider.IsConfigured)
            throw new InvalidOperationException(
                $"AI provider '{providerName}' is not configured. Please add API keys in settings.");

        return provider;
    }

    private static AiGenerationResultDto MapToDto(AiGenerationRecord r) => new()
    {
        Id = r.Id,
        Provider = r.Provider.ToString(),
        OperationType = r.OperationType.ToString(),
        Status = r.Status.ToString(),
        Prompt = r.Prompt,
        ResultContent = r.ResultContent,
        ResultImageUrl = r.ResultImageUrl,
        ErrorMessage = r.ErrorMessage,
        ModelName = r.ModelName,
        InputTokens = r.InputTokens,
        OutputTokens = r.OutputTokens,
        DurationSeconds = r.DurationSeconds,
        CreatedAt = r.CreatedAt,
        CompletedAt = r.CompletedAt
    };
}
