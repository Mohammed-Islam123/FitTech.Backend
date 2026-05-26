namespace Aggregation.Domain.Entities;

/// <description>
/// Monthly breakdown of revenue, new members, and sessions.
/// </description>
public class MonthlyBreakdown
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int NewMembers { get; set; }
    public int TotalSessions { get; set; }
}
