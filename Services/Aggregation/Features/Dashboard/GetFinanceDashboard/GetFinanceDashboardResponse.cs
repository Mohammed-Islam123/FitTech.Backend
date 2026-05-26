namespace Aggregation.Features.Dashboard.GetFinanceDashboard;

public record GetFinanceDashboardResponse(
    decimal MonthlyRevenue,
    int TotalPayments,
    decimal PlatformCommission,
    decimal NetProfits,
    decimal LifetimeRevenue
);
