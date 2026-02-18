using Microsoft.AspNetCore.Authorization;

namespace Portfolio.Api.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(policy: $"Permission:{permission}") { }
}
