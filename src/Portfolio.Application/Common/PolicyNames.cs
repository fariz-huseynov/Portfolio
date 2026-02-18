namespace Portfolio.Application.Common;

public static class PolicyNames
{
    // CORS
    public const string AllowFrontend = "AllowFrontend";

    // Output Cache
    public const string PublicContent = "PublicContent";
    public const string PublicContentTag = "public-content";

    // Rate Limiting
    public const string RateLimitLeadSubmit = "LeadSubmit";
    public const string RateLimitAuth = "Auth";
    public const string RateLimitPublicApi = "PublicApi";
    public const string RateLimitForgotPassword = "ForgotPassword";
    public const string RateLimitTwoFactorVerify = "TwoFactorVerify";
    public const string RateLimitAiGeneration = "AiGeneration";

    // SignalR Groups
    public const string AdminsGroup = "Admins";
}
