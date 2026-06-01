using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Courses.Domain.Configurations;

public class CoursePurchaseRequestConfiguration : IEntityTypeConfiguration<CoursePurchaseRequest>
{
    public void Configure(EntityTypeBuilder<CoursePurchaseRequest> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PaymentMethod).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Status).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.HasOne(r => r.Program).WithMany().HasForeignKey(r => r.ProgramId);
    }
}
