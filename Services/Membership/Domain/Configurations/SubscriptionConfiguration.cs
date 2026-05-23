using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Enums;

namespace Membership.Domain.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId)
            .IsRequired(false);

        builder.Property(x => x.PaymentStatus)
            .HasConversion<string>()
            .HasDefaultValue(PaymentStatus.Pending);
    }
}
