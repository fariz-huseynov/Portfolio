namespace Portfolio.Infrastructure.Identity;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Portfolio";
    public bool UseSsl { get; set; } = true;
    public bool Enabled { get; set; } = true;
}
