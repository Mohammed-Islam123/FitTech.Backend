using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Membership.Domain;

public class MembershipContextFactory : IDesignTimeDbContextFactory<MembershipDbContext>
{
    public MembershipDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MembershipDbContext>();
        // Hardcode a dummy string. The tool just needs to know
        // it's Npgsql so it can generate the right SQL syntax.
        optionsBuilder.UseNpgsql("Host=localhost;Database=membership-db");

        return new MembershipDbContext(optionsBuilder.Options);
    }
}
