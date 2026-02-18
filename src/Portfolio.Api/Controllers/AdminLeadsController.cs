using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Authorization;
using Portfolio.Application.DTOs;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.Interfaces;
using Asp.Versioning;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/leads")]
public class AdminLeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public AdminLeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    [HasPermission(Permissions.LeadsView)]
    public async Task<ActionResult<IReadOnlyList<LeadDto>>> GetAll(CancellationToken ct)
    {
        var leads = await _leadService.GetAllLeadsAsync(ct);
        return Ok(leads);
    }

    [HttpGet("paged")]
    [HasPermission(Permissions.LeadsView)]
    public async Task<ActionResult<PagedResult<LeadDto>>> GetAllPaged(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _leadService.GetAllLeadsPagedAsync(pagination, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    [HasPermission(Permissions.LeadsMarkRead)]
    public async Task<ActionResult<LeadDto>> MarkAsRead(Guid id, CancellationToken ct)
    {
        var lead = await _leadService.MarkAsReadAsync(id, ct);
        return Ok(lead);
    }
}
