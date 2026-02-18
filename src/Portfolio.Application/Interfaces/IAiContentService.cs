using Portfolio.Application.DTOs.AiContent;
using Portfolio.Application.DTOs.Pagination;

namespace Portfolio.Application.Interfaces;

public interface IAiContentService
{
    Task<AiGenerationResultDto> GenerateTextAsync(
        AiGenerateTextRequestDto request, string userId, CancellationToken ct = default);

    Task<AiGenerationResultDto> RewriteTextAsync(
        AiRewriteTextRequestDto request, string userId, CancellationToken ct = default);

    Task<AiGenerationResultDto> GenerateImageAsync(
        AiGenerateImageRequestDto request, string userId, CancellationToken ct = default);

    Task<AiGenerationResultDto?> GetGenerationByIdAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<AiGenerationResultDto>> GetGenerationHistoryAsync(
        PaginationParams pagination, CancellationToken ct = default);

    Task<IReadOnlyList<AiProviderStatusDto>> GetAvailableProvidersAsync(CancellationToken ct = default);
}
