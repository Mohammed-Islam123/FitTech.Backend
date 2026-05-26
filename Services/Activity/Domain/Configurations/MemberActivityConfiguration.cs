using Activity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Activity.Domain.Configurations;

public class MemberActivityConfiguration : IEntityTypeConfiguration<MemberActivity>
{
    public void Configure(EntityTypeBuilder<MemberActivity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.MemberId).IsRequired();
        builder.Property(a => a.CardUid).HasMaxLength(100);
        builder.HasIndex(a => a.MemberId);
        builder.HasIndex(a => a.CardUid);
        builder.HasIndex(a => a.CheckInTime);
    }
}
