using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Portfolio.Application.Common;

namespace Portfolio.Api.Hubs;

[Authorize]
public class AdminNotificationHub : Hub
{
    private readonly ILogger<AdminNotificationHub> _logger;

    public AdminNotificationHub(ILogger<AdminNotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation(
            "Admin connected. ConnectionId: {ConnectionId}, UserId: {UserId}",
            Context.ConnectionId,
            userId ?? "unknown");

        await Groups.AddToGroupAsync(Context.ConnectionId, PolicyNames.AdminsGroup);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Admin disconnected. ConnectionId: {ConnectionId}",
            Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, PolicyNames.AdminsGroup);
        await base.OnDisconnectedAsync(exception);
    }
}
