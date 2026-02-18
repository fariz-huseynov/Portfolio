namespace Portfolio.Application.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames, CancellationToken ct = default);
    void InvalidateCache();
}
