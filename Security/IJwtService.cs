
using System.Security.Claims;
using NetcoreHRIS.Entities;
namespace NetcoreHRIS.Security;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<Permission> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateRefreshToken(string token);
    Guid? GetUserIdFromToken(string token);
}