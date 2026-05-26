using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Courses.Domain.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Program).WithMany(p => p.Sessions).HasForeignKey(s => s.ProgramId);
        builder.HasOne(s => s.TimeSlot).WithMany(t => t.Sessions).HasForeignKey(s => s.TimeSlotId);
    }
}
