using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membership.Domain.Configurations;

public class PaymentApprovalRequestConfiguration : IEntityTypeConfiguration<PaymentApprovalRequest>
{
    public void Configure(EntityTypeBuilder<PaymentApprovalRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MemberId).IsRequired();
        builder.Property(r => r.RequestType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.ReferenceId).IsRequired();
        builder.Property(r => r.Status).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.Property(r => r.CreatedAt).IsRequired();
    }
}
