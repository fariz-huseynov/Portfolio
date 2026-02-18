namespace Portfolio.Application.Interfaces;

public interface ICaptchaService
{
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
}
