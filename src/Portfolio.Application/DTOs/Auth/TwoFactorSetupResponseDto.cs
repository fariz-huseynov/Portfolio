namespace Portfolio.Application.DTOs.Auth;

public class TwoFactorSetupResponseDto
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
}
