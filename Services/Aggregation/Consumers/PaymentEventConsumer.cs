using Aggregation.Domain;
using Aggregation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace Aggregation.Consumers;

public class PaymentEventConsumer(AggregationDbContext context)
{
    public async Task Handle(PaymentConfirmedEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        stats.TotalPayments++;
        stats.TotalRevenue += evt.Amount;
        stats.MonthlyRevenue += evt.Amount;
        stats.LastUpdated = DateTime.UtcNow;
        await UpsertMonthly(context, evt.CreatedAt.Year, evt.CreatedAt.Month, revenue: evt.Amount);
        await context.SaveChangesAsync();
    }

    public async Task Handle(MembershipRenewalAcceptedEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        stats.TotalRevenue += evt.Amount;
        stats.MonthlyRevenue += evt.Amount;
        stats.ActiveSubscriptions++;
        stats.LastUpdated = DateTime.UtcNow;
        await UpsertMonthly(context, evt.AcceptedAt.Year, evt.AcceptedAt.Month, revenue: evt.Amount);
        await context.SaveChangesAsync();
    }

    public async Task Handle(CoursePurchaseAcceptedEvent evt)
    {
        var stats = await GetOrCreateStats(context);
        stats.TotalRevenue += evt.Amount;
        stats.MonthlyRevenue += evt.Amount;
        stats.LastUpdated = DateTime.UtcNow;
        await UpsertMonthly(context, evt.AcceptedAt.Year, evt.AcceptedAt.Month, revenue: evt.Amount);
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

    private static async Task UpsertMonthly(AggregationDbContext ctx, int year, int month, decimal revenue = 0, int newMembers = 0)
    {
        var mb = await ctx.MonthlyBreakdowns.FirstOrDefaultAsync(m => m.Year == year && m.Month == month);
        if (mb is null)
        {
            mb = new MonthlyBreakdown { Year = year, Month = month };
            ctx.MonthlyBreakdowns.Add(mb);
        }
        mb.Revenue += revenue;
        mb.NewMembers += newMembers;
    }
}
