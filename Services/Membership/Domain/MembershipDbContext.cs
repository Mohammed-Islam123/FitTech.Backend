using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Membership.Domain;

public sealed class MembershipDbContext(DbContextOptions<MembershipDbContext> options) : DbContext(options)
{
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<MemberHealthProfile> MemberHealthProfiles { get; set; } = null!;
    public DbSet<NfcCard> NfcCards { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MembershipDbContext).Assembly);
    }
}