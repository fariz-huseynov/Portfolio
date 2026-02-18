using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Authorization;
using Portfolio.Application.DTOs.Pagination;
using Portfolio.Application.DTOs.Security;
using Portfolio.Application.Interfaces;
using Asp.Versioning;
using Portfolio.Domain.Constants;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/admin/ip-rules")]
public class AdminIpRulesController : ControllerBase
{
    private readonly IIpRuleService _ipRuleService;

    public AdminIpRulesController(IIpRuleService ipRuleService)
    {
        _ipRuleService = ipRuleService;
    }

    [HttpGet]
    [HasPermission(Permissions.SecurityView)]
    public async Task<ActionResult<PagedResult<IpRuleDto>>> GetRules(
        [FromQuery] IpRuleFilterParams filter, CancellationToken ct)
    {
        var result = await _ipRuleService.GetRulesPagedAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.SecurityView)]
    public async Task<ActionResult<IpRuleDto>> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _ipRuleService.GetRuleByIdAsync(id, ct);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    [HasPermission(Permissions.SecurityManage)]
    public async Task<ActionResult<IpRuleDto>> Create(
        [FromBody] IpRuleCreateDto dto, CancellationToken ct)
    {
        var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var rule = await _ipRuleService.CreateRuleAsync(dto, createdBy, ct);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.SecurityManage)]
    public async Task<ActionResult<IpRuleDto>> Update(
        Guid id, [FromBody] IpRuleUpdateDto dto, CancellationToken ct)
    {
        var rule = await _ipRuleService.UpdateRuleAsync(id, dto, ct);
        return Ok(rule);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.SecurityManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _ipRuleService.DeleteRuleAsync(id, ct);
        return NoContent();
    }
}
