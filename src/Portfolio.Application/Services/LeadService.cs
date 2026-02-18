using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;

namespace Portfolio.Application.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IAdminNotificationService _notificationService;

    public LeadService(ILeadRepository leadRepository, IAdminNotificationService notificationService)
    {
        _leadRepository = leadRepository;
        _notificationService = notificationService;
    }

    public async Task<LeadDto> SubmitLeadAsync(LeadSubmitDto dto, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = dto.Company,
            Message = dto.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _leadRepository.AddAsync(lead, ct);

        // Fire-and-forget notification — failure must not break lead submission
        _ = _notificationService.NotifyNewLeadAsync(lead.FullName, lead.Email, ct);

        return MapToDto(lead);
    }

    public async Task<IReadOnlyList<LeadDto>> GetAllLeadsAsync(CancellationToken ct = default)
    {
        var leads = await _leadRepository.GetAllOrderedByDateAsync(ct);
        return leads.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<LeadDto>> GetAllLeadsPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var (items, totalCount) = await _leadRepository.GetAllOrderedByDatePagedAsync(pagination.Page, pagination.PageSize, ct);
        return PagedResult<LeadDto>.Create(items.Select(MapToDto).ToList(), pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<LeadDto> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var lead = await _leadRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Lead with ID {id} not found.");

        lead.IsRead = true;
        lead.ReadAt = DateTime.UtcNow;
        lead.UpdatedAt = DateTime.UtcNow;

        await _leadRepository.UpdateAsync(lead, ct);
        return MapToDto(lead);
    }

    private static LeadDto MapToDto(Lead lead) => new()
    {
        Id = lead.Id,
        FullName = lead.FullName,
        Email = lead.Email,
        Phone = lead.Phone,
        Company = lead.Company,
        Message = lead.Message,
        IsRead = lead.IsRead,
        ReadAt = lead.ReadAt,
        CreatedAt = lead.CreatedAt
    };
}
