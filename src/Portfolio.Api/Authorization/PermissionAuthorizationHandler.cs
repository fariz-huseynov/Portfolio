using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var roleClaims = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (roleClaims.Count == 0)
            return;

        using var scope = _scopeFactory.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var permissions = await permissionService.GetPermissionsForRolesAsync(roleClaims);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
