using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.DTOs;
using Asp.Versioning;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/leads")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly ICaptchaService _captchaService;

    public LeadsController(ILeadService leadService, ICaptchaService captchaService)
    {
        _leadService = leadService;
        _captchaService = captchaService;
    }

    [HttpPost("submit")]
    [AllowAnonymous]
    [EnableRateLimiting("LeadSubmit")]
    public async Task<ActionResult<LeadDto>> Submit(
        [FromBody] LeadSubmitDto dto,
        CancellationToken ct)
    {
        var captchaToken = $"{dto.CaptchaId}:{dto.CaptchaCode}";
        var isHuman = await _captchaService.ValidateTokenAsync(captchaToken, ct);
        if (!isHuman)
            return BadRequest(new { error = "CAPTCHA verification failed. Please try again." });

        var result = await _leadService.SubmitLeadAsync(dto, ct);
        return Ok(result);
    }
}
