namespace Aggregation.Domain.Entities;

/// <description>
/// Membership plan distribution for pie chart.
/// </description>
public class PlansBreakdown
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string PlanName { get; set; } = null!;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
