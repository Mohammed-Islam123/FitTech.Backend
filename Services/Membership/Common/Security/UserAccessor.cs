using System.Security.Claims;

namespace Membership.Common.Security;

public interface IUserAccessor
{
    Guid? UserId { get; }
    IEnumerable<string> Roles { get; }
    bool IsAdmin { get; }
    bool IsCoach { get; }
    bool IsMember { get; }
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

    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsCoach => Roles.Contains("Coach");
    public bool IsMember => Roles.Contains("Member");
}
