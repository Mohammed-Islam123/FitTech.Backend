using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Plans.ListPlans;

public class ListPlansHandler(MembershipDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListPlansResponse>>> Handle(ListPlansQuery query, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized("Plan.Unauthorized", "Only Administrators can list subscription plans.");
        }

        return await context.SubscriptionPlans
            .AsNoTracking()
            .Select(p => new ListPlansResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.DurationValue,
                p.DurationUnit,
                p.SessionCount,
                p.AccessRules,
                p.IsActive))
            .ToListAsync(ct);
    }
}
