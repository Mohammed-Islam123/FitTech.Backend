namespace Aggregation.Domain.Entities;

/// <description>
/// Singleton read model for aggregated dashboard statistics.
/// Id=1 is the only row — updated by event consumers.
/// </description>
public class AggregatedStats
{
    public int Id { get; set; } = 1;
    public int TotalMembers { get; set; }
    public int TodayCheckIns { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TotalPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalSubscriptions { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
