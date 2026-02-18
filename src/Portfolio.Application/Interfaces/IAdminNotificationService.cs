namespace Portfolio.Application.Interfaces;

public interface IAdminNotificationService
{
    Task NotifyNewLeadAsync(string leadName, string leadEmail, CancellationToken ct = default);
}
