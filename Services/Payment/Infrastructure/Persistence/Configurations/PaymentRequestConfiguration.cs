using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Persistence.Configurations;

public class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
{
    public void Configure(EntityTypeBuilder<PaymentRequest> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.MemberId).IsRequired();
        builder.Property(r => r.RequestType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(r => r.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.ReferenceId).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.Property(r => r.CreatedAt).IsRequired();
    }
}
