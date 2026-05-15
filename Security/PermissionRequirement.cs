using Microsoft.AspNetCore.Authorization;

namespace NetcoreHRIS.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Action { get; }
    public string Subject { get; }

    public PermissionRequirement(string action, string subject)
    {
        Action = action;
        Subject = subject;
    }
}
