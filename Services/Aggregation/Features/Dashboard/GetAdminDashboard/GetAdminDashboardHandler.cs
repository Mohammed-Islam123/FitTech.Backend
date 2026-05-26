using Aggregation.Common.Security;
using Aggregation.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Aggregation.Features.Dashboard.GetAdminDashboard;

public class GetAdminDashboardHandler(AggregationDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetAdminDashboardResponse>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Dashboard.Unauthorized", "Only Administrators can view the dashboard.");

        var stats = await context.AggregatedStats.FindAsync([1], ct);

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var growth = await context.MonthlyBreakdowns
            .AsNoTracking()
            .Where(m => m.Year >= sixMonthsAgo.Year || (m.Year == sixMonthsAgo.Year && m.Month >= sixMonthsAgo.Month))
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .Select(m => new MembershipGrowthPoint($"{m.Year}-{m.Month:D2}", m.NewMembers))
            .ToListAsync(ct);

        var totalSubscriptions = stats?.TotalSubscriptions ?? 0;
        var plans = await context.PlansBreakdowns
            .AsNoTracking()
            .Select(p => new PlansBreakdownItem(p.PlanName, p.Percentage))
            .ToListAsync(ct);

        return new GetAdminDashboardResponse(
            stats?.TotalMembers ?? 0,
            stats?.TodayCheckIns ?? 0,
            stats?.MonthlyRevenue ?? 0,
            stats?.ActiveSubscriptions ?? 0,
            growth,
            plans);
    }
}
