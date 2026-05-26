using System.Security.Claims;

namespace Payment.Common.Security;

public interface IUserAccessor
{
    Guid? UserId { get; }
    IEnumerable<string> Roles { get; }
    bool IsAdmin { get; }
    bool IsMember { get; }
    bool IsInRole(string role);
}

public class UserAccessor(IHttpContextAccessor httpContextAccessor) : IUserAccessor
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User?.FindFirstValue("sub");
            return Guid.TryParse(userIdClaim, out var guid) ? guid : null;
        }
    }

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];

    public bool IsAdmin => IsInRole("Admin");
    public bool IsMember => IsInRole("Member");

    public bool IsInRole(string role) => Roles.Contains(role);
}
