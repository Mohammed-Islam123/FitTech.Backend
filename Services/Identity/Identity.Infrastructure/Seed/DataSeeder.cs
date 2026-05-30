using Identity.Domain.Enums;
using Identity.Infrastructure.Identity;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(
        UserDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedClientsAsync(context);
        await SeedUsersAsync(userManager);
    }

    // Roles 

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = ["Admin", "Member", "Coach"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    Description = role switch
                    {
                        "Admin"  => "Administrator with full permissions",
                        "Member" => "Gym member with standard access",
                        "Coach"  => "Coach who can manage training sessions",
                        _        => string.Empty
                    }
                });
            }
        }
    }

    // Clients 

    private static async Task SeedClientsAsync(UserDbContext context)
    {
        var clients = new List<Client>
        {
            new() { ClientId = "web",     ClientName = "Web Client",     Description = "Web browser clients", IsActive = true },
            new() { ClientId = "android", ClientName = "Android Client", Description = "Android mobile app",  IsActive = true },
            new() { ClientId = "ios",     ClientName = "iOS Client",     Description = "iOS mobile app",      IsActive = true },
        };

        foreach (var client in clients)
        {
            if (!await context.Clients.AnyAsync(c => c.ClientId == client.ClientId))
                await context.Clients.AddAsync(client);
        }

        await context.SaveChangesAsync();
    }

    // Users 

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var adminUser = new ApplicationUser
        {
            UserName        = "admin",
            Email           = "Admin@fitteck.com",
            FirstName       = "FitTeck",
            LastName        = "Admin",
            IsActive        = true,
            EmailConfirmed  = true,
            IsEmailConfirmed = true,
            CreatedAt       = DateTime.UtcNow,
            Gender          = Gender.Male,
        };

        if (await userManager.FindByEmailAsync(adminUser.Email!) is null)
        {
            var result = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}