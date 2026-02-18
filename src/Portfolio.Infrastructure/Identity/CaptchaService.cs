using Lazy.Captcha.Core;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.Identity;

public class CaptchaService : ICaptchaService
{
    private readonly ICaptcha _captcha;
    private readonly ILogger<CaptchaService> _logger;

    public CaptchaService(ICaptcha captcha, ILogger<CaptchaService> logger)
    {
        _captcha = captcha;
        _logger = logger;
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        // Token format: "id:code"
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("CAPTCHA validation failed: empty token");
            return Task.FromResult(false);
        }

        var parts = token.Split(':', 2);
        if (parts.Length != 2)
        {
            _logger.LogWarning("CAPTCHA validation failed: invalid token format");
            return Task.FromResult(false);
        }

        var id = parts[0];
        var code = parts[1];

        var isValid = _captcha.Validate(id, code);

        if (!isValid)
        {
            _logger.LogWarning("CAPTCHA validation failed for id: {Id}", id);
        }

        return Task.FromResult(isValid);
    }
}
