using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;

namespace Portfolio.Application.Interfaces;

public interface ILeadService
{
    Task<LeadDto> SubmitLeadAsync(LeadSubmitDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<LeadDto>> GetAllLeadsAsync(CancellationToken ct = default);
    Task<PagedResult<LeadDto>> GetAllLeadsPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<LeadDto> MarkAsReadAsync(Guid id, CancellationToken ct = default);
}
