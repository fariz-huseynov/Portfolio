namespace Portfolio.Application.DTOs.Auth;

public class TwoFactorEnableResponseDto
{
    public IReadOnlyList<string> RecoveryCodes { get; set; } = [];
}
