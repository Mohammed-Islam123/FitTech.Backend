using Aggregation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aggregation.Domain;

public sealed class AggregationDbContext(DbContextOptions<AggregationDbContext> options) : DbContext(options)
{
    public DbSet<AggregatedStats> AggregatedStats { get; set; } = null!;
    public DbSet<MonthlyBreakdown> MonthlyBreakdowns { get; set; } = null!;
    public DbSet<PlansBreakdown> PlansBreakdowns { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AggregatedStats>().HasKey(s => s.Id);
        modelBuilder.Entity<AggregatedStats>().Property(s => s.MonthlyRevenue).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<AggregatedStats>().Property(s => s.TotalRevenue).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<MonthlyBreakdown>().HasKey(m => m.Id);
        modelBuilder.Entity<MonthlyBreakdown>().Property(m => m.Revenue).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<MonthlyBreakdown>().HasIndex(m => new { m.Year, m.Month }).IsUnique();
        modelBuilder.Entity<PlansBreakdown>().HasKey(p => p.Id);
        modelBuilder.Entity<PlansBreakdown>().Property(p => p.Percentage).HasColumnType("decimal(5,2)");
    }
}
