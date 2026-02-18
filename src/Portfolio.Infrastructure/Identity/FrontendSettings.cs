namespace Portfolio.Infrastructure.Identity;

public class FrontendSettings
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; set; } = string.Empty;
    public string ResetPasswordPath { get; set; } = "/reset-password";
}
