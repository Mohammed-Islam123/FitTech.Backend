using Aggregation.Common.Security;
using Aggregation.Domain;
using ErrorOr;

namespace Aggregation.Features.Dashboard.GetFinanceDashboard;

public class GetFinanceDashboardHandler(AggregationDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetFinanceDashboardResponse>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Dashboard.Unauthorized", "Only Administrators can view finance data.");

        var stats = await context.AggregatedStats.FindAsync([1], ct);

        var monthlyRevenue = stats?.MonthlyRevenue ?? 0;
        var lifetimeRevenue = stats?.TotalRevenue ?? 0;
        var totalPayments = stats?.TotalPayments ?? 0;
        var commission = monthlyRevenue * 0.10m; // 10% platform commission
        var netProfits = monthlyRevenue - commission;

        return new GetFinanceDashboardResponse(
            monthlyRevenue, totalPayments, commission, netProfits, lifetimeRevenue);
    }
}
