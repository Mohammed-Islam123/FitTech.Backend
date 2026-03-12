using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Infrastructure.Identity;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Persistence;

public class UserDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<MedicalFile> MedicalFiles { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.ClientId);

            entity.Property(c => c.ClientId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.ClientName)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(c => c.ClientName)
                .IsUnique()
                .HasDatabaseName("Index_ClientName_Unique");

            entity.Property(c => c.IsActive)
                .HasDefaultValue(true);

            entity.HasMany(c => c.RefreshTokens)
                .WithOne(rt => rt.Client)
                .HasForeignKey(rt => rt.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token)
                .IsRequired();

            entity.HasIndex(rt => rt.Token)
                .IsUnique();

            entity.Property(rt => rt.UserId)
                .IsRequired();

            entity.Property(rt => rt.ClientId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(rt => rt.UserAgent)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(rt => rt.CreatedByIp)
                .IsRequired()
                .HasMaxLength(45);

            entity.Property(rt => rt.RevokedByIp)
                .HasMaxLength(45);

            entity.Property(rt => rt.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Ignore(rt => rt.IsActive);
            entity.Ignore(rt => rt.IsExpired);

            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MedicalFile>(entity =>
        {
            entity.HasKey(mf => mf.Id);

            entity.Property(mf => mf.UserId)
                .IsRequired();

            entity.Property(mf => mf.FileName)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(mf => mf.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(mf => mf.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(mf => mf.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.MedicalFiles)
                .HasForeignKey(mf => mf.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);

            entity.Property(u => u.DateOfBirth);
        });

        
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
    }
}