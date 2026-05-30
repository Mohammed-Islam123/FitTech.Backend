using Membership.Common.Security;

namespace Membership.Infrastructure.Seed;

public sealed class SeedUserAccessor(Guid? userId, IEnumerable<string> roles) : IUserAccessor
{
    public Guid? UserId { get; } = userId;
    public IEnumerable<string> Roles { get; } = roles.ToArray();

    public bool IsAdmin => IsInRole("Admin");
    public bool IsCoach => IsInRole("Coach");
    public bool IsMember => IsInRole("Member");

    public bool IsInRole(string role) => Roles.Contains(role);
}
