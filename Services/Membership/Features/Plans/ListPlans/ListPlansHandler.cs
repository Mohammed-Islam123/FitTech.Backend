using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Plans.ListPlans;

public class ListPlansHandler(MembershipDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListPlansResponse>>> Handle(ListPlansQuery query, CancellationToken ct)
    {
        var queryable = context.SubscriptionPlans.AsNoTracking();

        if (!userAccessor.IsAdmin)
        {
            // Non-admin users (members) see only active plans
            queryable = queryable.Where(p => p.IsActive);
        }

        return await queryable
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
