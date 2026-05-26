using Aggregation.Domain;
using Aggregation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace Aggregation.Consumers;

public class ActivityEventConsumer(AggregationDbContext context)
{
    public async Task Handle(MemberCheckedInEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        stats.TodayCheckIns++;
        stats.LastUpdated = DateTime.UtcNow;
        await UpsertMonthly(context, evt.CheckInTime.Year, evt.CheckInTime.Month, sessions: 1);
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

    private static async Task UpsertMonthly(AggregationDbContext ctx, int year, int month, int sessions = 0)
    {
        var mb = await ctx.MonthlyBreakdowns.FirstOrDefaultAsync(m => m.Year == year && m.Month == month);
        if (mb is null)
        {
            mb = new MonthlyBreakdown { Year = year, Month = month };
            ctx.MonthlyBreakdowns.Add(mb);
        }
        mb.TotalSessions += sessions;
    }
}
