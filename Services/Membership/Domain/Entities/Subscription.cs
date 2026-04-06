using Membership.Domain.Enums;

namespace Membership.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartOnUTC { get; set; }
    public DateTime? EndOnUTC { get; set; }
    public int? RemainingSessions { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime? CancelledAt { get; set; }
    public DateTime? PausedUntil { get; set; }

    public Member Member { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}

