using System.Security.Claims;

namespace Activity.Common.Security;

public interface IUserAccessor
{
    Guid? UserId { get; }
    IEnumerable<string> Roles { get; }
    bool IsAdmin { get; }
    bool IsCoach { get; }
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
            var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");
            return Guid.TryParse(claim, out var g) ? g : null;
        }
    }
    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];
    public bool IsAdmin => IsInRole("Admin");
    public bool IsCoach => IsInRole("Coach");
    public bool IsMember => IsInRole("Member");
    public bool IsInRole(string role) => Roles.Contains(role);
}
