using Membership.Domain.Enums;
using Shared.Enums;

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
    public bool AutoRenew { get; set; }
    public Guid? PaymentId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public Member Member { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}
