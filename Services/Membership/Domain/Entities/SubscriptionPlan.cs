using Membership.Domain.Enums;

namespace Membership.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? DurationValue { get; set; }
    public DurationUnit? DurationUnit { get; set; }
    public int? SessionCount { get; set; }
    public string? AccessRules { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Subscription> Subscriptions { get; set; } = [];
}

