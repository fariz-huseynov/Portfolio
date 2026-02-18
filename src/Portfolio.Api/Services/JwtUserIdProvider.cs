using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Portfolio.Api.Services;

public class JwtUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
