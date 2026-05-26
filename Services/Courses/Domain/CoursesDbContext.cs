using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Courses.Domain;

public sealed class CoursesDbContext(DbContextOptions<CoursesDbContext> options) : DbContext(options)
{
    public DbSet<Coach> Coaches { get; set; } = null!;
    public DbSet<ProgramEntity> Programs { get; set; } = null!;
    public DbSet<ProgramTimeSlot> ProgramTimeSlots { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<ProgramEnrollment> ProgramEnrollments { get; set; } = null!;
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoursesDbContext).Assembly);
    }
}
