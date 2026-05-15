using Microsoft.AspNetCore.Authorization;

namespace NetcoreHRIS.Security;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string action, string subject)
        : base(policy: $"{action}:{subject}")
    {
    }
}