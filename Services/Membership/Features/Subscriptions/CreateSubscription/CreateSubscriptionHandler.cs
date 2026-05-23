using ErrorOr;
using Membership.Domain;
using Membership.Domain.Entities;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Membership.Features.Subscriptions.CreateSubscription;

public class CreateSubscriptionHandler(MembershipDbContext context)
{
    public async Task<ErrorOr<CreateSubscriptionResponse>> Handle(
        CreateSubscriptionCommand command,
        CancellationToken ct)
    {
        var req = command.Request;

        var member = await context.Members.FindAsync([req.MemberId], cancellationToken: ct);
        if (member is null || member.Status != MemberStatus.Active)
        {
            return Error.NotFound("Member.NotFound", "Member does not exist or is inactive.");
        }

        var plan = await context.SubscriptionPlans.FindAsync([req.PlanId], cancellationToken: ct);
        if (plan is null || !plan.IsActive)
        {
            return Error.NotFound("SubscriptionPlan.NotFound", "Plan does not exist or is inactive.");
        }

        var existingSubscription = await context.Subscriptions
            .Where(s => s.MemberId == req.MemberId && s.Status == SubscriptionStatus.Active)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (existingSubscription is not null)
        {
            return Error.Conflict("Subscription.AlreadyActive", "Member already has an active subscription.");
        }

        var startOnUtc = DateTime.UtcNow;
        DateTime? endOnUtc = null;

        if (plan.DurationUnit == DurationUnit.Months && plan.DurationValue.HasValue)
        {
            endOnUtc = startOnUtc.AddMonths(plan.DurationValue.Value);
        }
        else if (plan.DurationUnit == DurationUnit.Days && plan.DurationValue.HasValue)
        {
            endOnUtc = startOnUtc.AddDays(plan.DurationValue.Value);
        }

        var subscription = new Subscription
        {
            Id = Guid.CreateVersion7(),
            MemberId = req.MemberId,
            PlanId = req.PlanId,
            StartOnUTC = startOnUtc,
            EndOnUTC = endOnUtc,
            RemainingSessions = plan.SessionCount,
            Status = SubscriptionStatus.Active,
            AutoRenew = false,
            PaymentId = null,
            PaymentStatus = PaymentStatus.Pending
        };

        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync(ct);

        return new CreateSubscriptionResponse(
            subscription.Id,
            subscription.PaymentStatus);
    }
}
