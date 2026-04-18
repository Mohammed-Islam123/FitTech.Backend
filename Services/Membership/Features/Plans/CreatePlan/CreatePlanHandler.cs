using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;

namespace Membership.Features.Plans.CreatePlan;

public class CreatePlanHandler(MembershipDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<CreatePlanResponse>> Handle(CreatePlanCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized("Plan.Unauthorized", "Only Administrators can create subscription plans.");
        }

        var req = command.Request;

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            DurationValue = req.DurationValue,
            DurationUnit = req.DurationUnit,
            SessionCount = req.SessionCount,
            AccessRules = req.AccessRules,
            IsActive = true
        };

        context.SubscriptionPlans.Add(plan);
        await context.SaveChangesAsync(ct);

        return new CreatePlanResponse(plan.Id);
    }
}
