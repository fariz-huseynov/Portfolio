using Microsoft.AspNetCore.SignalR;
using Portfolio.Api.Hubs;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Services;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly IHubContext<AdminNotificationHub> _hubContext;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        IHubContext<AdminNotificationHub> hubContext,
        ILogger<AdminNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewLeadAsync(string leadName, string leadEmail, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group("Admins").SendAsync(
                "NewLeadReceived",
                new { Name = leadName, Email = leadEmail, ReceivedAt = DateTime.UtcNow },
                ct);

            _logger.LogInformation("Notified admins of new lead from {LeadName}", leadName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SignalR notification for lead from {LeadName}", leadName);
        }
    }
}
