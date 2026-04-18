using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure;

public class IdentityDbFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        // Hardcode a dummy string. The tool just needs to know
        // it's Npgsql so it can generate the right SQL syntax.
        optionsBuilder.UseNpgsql("Host=localhost;Database=thisistest");

        return new UserDbContext(optionsBuilder.Options);
    }
}
