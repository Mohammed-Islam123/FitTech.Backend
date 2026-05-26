using Aggregation.Domain;
using Aggregation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace Aggregation.Consumers;

/// <description>
/// Consumes member-related events to update aggregated read models.
/// </description>
public class MemberEventConsumer(AggregationDbContext context)
{
    public async Task Handle(MemberCreatedEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        stats.TotalMembers++;
        stats.TotalUsers++;
        stats.TotalRegistrations++;
        stats.ActiveSubscriptions++;
        stats.LastUpdated = DateTime.UtcNow;

        await UpsertMonthly(context, DateTime.UtcNow.Year, DateTime.UtcNow.Month, newMembers: 1);
        await context.SaveChangesAsync();
    }

    public async Task Handle(MemberSuspendedEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        if (stats.ActiveSubscriptions > 0) stats.ActiveSubscriptions--;
        stats.LastUpdated = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task Handle(MemberActivatedEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        stats.ActiveSubscriptions++;
        stats.LastUpdated = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    private static async Task<AggregatedStats> GetOrCreateStats(AggregationDbContext ctx)
    {
        var stats = await ctx.AggregatedStats.FindAsync(1);
        if (stats is null)
        {
            stats = new AggregatedStats();
            ctx.AggregatedStats.Add(stats);
        }
        return stats;
    }

    private static async Task UpsertMonthly(AggregationDbContext ctx, int year, int month, int newMembers = 0, decimal revenue = 0, int sessions = 0)
    {
        var mb = await ctx.MonthlyBreakdowns
            .FirstOrDefaultAsync(m => m.Year == year && m.Month == month);
        if (mb is null)
        {
            mb = new MonthlyBreakdown { Year = year, Month = month };
            ctx.MonthlyBreakdowns.Add(mb);
        }
        mb.NewMembers += newMembers;
        mb.Revenue += revenue;
        mb.TotalSessions += sessions;
    }
}
