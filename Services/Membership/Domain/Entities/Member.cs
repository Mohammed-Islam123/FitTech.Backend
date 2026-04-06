using Membership.Domain.Enums;

namespace Membership.Domain.Entities;

public class Member
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public int NoShowWarningCount { get; set; }
    public DateTime? PausedUntil { get; set; }

    public MemberHealthProfile? HealthProfile { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<NfcCard> NfcCards { get; set; } = [];
}

