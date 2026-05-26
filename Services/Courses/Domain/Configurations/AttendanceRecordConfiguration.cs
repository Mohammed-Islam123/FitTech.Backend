using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Courses.Domain.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(10);
        builder.HasOne(a => a.Session).WithMany(s => s.AttendanceRecords).HasForeignKey(a => a.SessionId);
        builder.HasIndex(a => new { a.SessionId, a.MemberId }).IsUnique();
    }
}
