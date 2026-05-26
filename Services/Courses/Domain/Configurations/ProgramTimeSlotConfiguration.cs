using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Courses.Domain.Configurations;

public class ProgramTimeSlotConfiguration : IEntityTypeConfiguration<ProgramTimeSlot>
{
    public void Configure(EntityTypeBuilder<ProgramTimeSlot> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Day).HasConversion<string>().HasMaxLength(10);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.HasOne(t => t.Program).WithMany(p => p.TimeSlots).HasForeignKey(t => t.ProgramId);
    }
}
