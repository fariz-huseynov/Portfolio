using Asp.Versioning;
using Lazy.Captcha.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Api.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{version:apiVersion}/captcha")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptcha _captcha;

    public CaptchaController(ICaptcha captcha)
    {
        _captcha = captcha;
    }

    [HttpGet("generate")]
    [AllowAnonymous]
    public IActionResult Generate()
    {
        var captchaId = Guid.NewGuid().ToString();
        var captchaData = _captcha.Generate(captchaId);

        return Ok(new
        {
            id = captchaData.Id,
            image = $"data:image/png;base64,{captchaData.Base64}"
        });
    }

    [HttpGet("image")]
    [AllowAnonymous]
    public IActionResult GetImage()
    {
        var captchaId = Guid.NewGuid().ToString();
        var captchaData = _captcha.Generate(captchaId);
        var imageBytes = Convert.FromBase64String(captchaData.Base64);

        return File(imageBytes, "image/png");
    }
}
