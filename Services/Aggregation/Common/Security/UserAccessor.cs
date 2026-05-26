using System.Security.Claims;

namespace Aggregation.Common.Security;

public interface IUserAccessor
{
    bool IsAdmin { get; }
}

public class UserAccessor(IHttpContextAccessor httpContextAccessor) : IUserAccessor
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
    public bool IsAdmin => User?.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin") ?? false;
}
