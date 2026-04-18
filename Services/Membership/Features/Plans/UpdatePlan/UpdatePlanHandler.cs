using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Plans.UpdatePlan;

public class UpdatePlanHandler(MembershipDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<Success>> Handle(UpdatePlanCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized("Plan.Unauthorized", "Only Administrators can update subscription plans.");
        }

        var plan = await context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (plan is null)
        {
            return Error.NotFound("Plan.NotFound", "The specified plan was not found.");
        }

        var req = command.Request;

        plan.Name = req.Name;
        plan.Description = req.Description;
        plan.Price = req.Price;
        plan.DurationValue = req.DurationValue;
        plan.DurationUnit = req.DurationUnit;
        plan.SessionCount = req.SessionCount;
        plan.AccessRules = req.AccessRules;
        plan.IsActive = req.IsActive;

        await context.SaveChangesAsync(ct);

        return Result.Success;
    }
}
