using Shared.Enums;

namespace Payment.Domain.Entities;

/// <remarks>
/// Represents a payment transaction for subscriptions, e-commerce, or per-session purchases.
/// </remarks>
public class Payment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    /// <remarks>
    /// Factory method to create a new Payment instance.
    /// </remarks>
    public static Payment Create(
        Guid userId,
        decimal amount,
        PaymentMethod paymentMethod,
        PaymentType paymentType,
        Guid referenceId,
        string? notes = null)
    {
        return new Payment
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Amount = amount,
            Currency = "DZD",
            Status = PaymentStatus.Paid,
            PaymentMethod = paymentMethod,
            PaymentType = paymentType,
            ReferenceId = referenceId,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}
