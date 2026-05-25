using System.Security.Claims;

namespace Chat.Common;

public class UserAccessor(IHttpContextAccessor http)
{
    public Guid UserId =>
        Guid.Parse(http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? http.HttpContext!.User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User not authenticated"));

    public string Role =>
        http.HttpContext!.User.FindFirstValue(ClaimTypes.Role)
        ?? http.HttpContext!.User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
        ?? string.Empty;

    public bool IsCoach => Role.Equals("Coach", StringComparison.OrdinalIgnoreCase);

    public string? Email => http.HttpContext!.User.FindFirstValue(ClaimTypes.Email)
        ?? http.HttpContext!.User.FindFirstValue("email");

    public string? Name => http.HttpContext!.User.FindFirstValue("name");
}