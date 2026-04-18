using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Plans.DeletePlan;

public class DeletePlanHandler(MembershipDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<Success>> Handle(DeletePlanCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized("Plan.Unauthorized", "Only Administrators can delete subscription plans.");
        }

        var plan = await context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (plan is null)
        {
            return Error.NotFound("Plan.NotFound", "The specified plan was not found.");
        }

        plan.IsActive = false; // Soft delete

        await context.SaveChangesAsync(ct);

        return Result.Success;
    }
}
