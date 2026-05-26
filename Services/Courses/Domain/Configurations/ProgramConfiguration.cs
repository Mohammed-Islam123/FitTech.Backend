using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Courses.Domain.Configurations;

public class ProgramConfiguration : IEntityTypeConfiguration<ProgramEntity>
{
    public void Configure(EntityTypeBuilder<ProgramEntity> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Level).HasMaxLength(50);
        builder.Property(p => p.ExerciseType).HasMaxLength(100);
        builder.Property(p => p.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne(p => p.Coach).WithMany(c => c.Programs).HasForeignKey(p => p.CoachId);
    }
}
