namespace Aggregation.Features.Dashboard.GetAdminDashboard;

public record GetAdminDashboardResponse(
    int TotalMembers,
    int TodayCheckIns,
    decimal MonthlyRevenue,
    int ActiveSubscriptions,
    List<MembershipGrowthPoint> MembershipGrowth,
    List<PlansBreakdownItem> PlansBreakdown
);

public record MembershipGrowthPoint(string Month, int Count);

public record PlansBreakdownItem(string PlanName, decimal Percentage);
