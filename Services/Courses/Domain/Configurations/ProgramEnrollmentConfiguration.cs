using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Courses.Domain.Configurations;

public class ProgramEnrollmentConfiguration : IEntityTypeConfiguration<ProgramEnrollment>
{
    public void Configure(EntityTypeBuilder<ProgramEnrollment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Program).WithMany(p => p.Enrollments).HasForeignKey(e => e.ProgramId);
        builder.HasIndex(e => new { e.ProgramId, e.MemberId }).IsUnique();
    }
}
