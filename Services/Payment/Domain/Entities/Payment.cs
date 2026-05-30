using Shared.Enums;

namespace Payment.Domain.Entities;

/// <description>
/// Represents a payment transaction. Cash payments go directly to Paid.
/// Online payments flow Pending → Processing → Paid (via gateway webhook).
/// </description>
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
    public string? GatewayTransactionId { get; internal set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    /// <description>
    /// Factory method. Cash payments start as Paid immediately.
    /// Online payments start as Pending and transition via Confirm/Fail.
    /// </description>
    public static Payment Create(
        Guid userId,
        decimal amount,
        PaymentMethod paymentMethod,
        PaymentType paymentType,
        Guid referenceId,
        string? notes = null)
    {
        var initialStatus = paymentMethod == PaymentMethod.Cash
            ? PaymentStatus.Paid
            : PaymentStatus.Pending;

        return new Payment
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Amount = amount,
            Currency = "DZD",
            Status = initialStatus,
            PaymentMethod = paymentMethod,
            PaymentType = paymentType,
            ReferenceId = referenceId,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <description>
    /// Transitions a pending online payment to Paid status. Idempotent.
    /// </description>
    public void Confirm(string? gatewayTransactionId = null)
    {
        if (Status is PaymentStatus.Paid) return;

        Status = PaymentStatus.Paid;
        GatewayTransactionId = gatewayTransactionId;
    }

    /// <description>
    /// Marks an online payment as failed.
    /// </description>
    public void MarkFailed(string? gatewayTransactionId = null)
    {
        Status = PaymentStatus.Cancelled;
        GatewayTransactionId = gatewayTransactionId;
    }

    /// <description>
    /// Requests a refund. Only callable on Paid payments.
    /// </description>
    public void Refund()
    {
        if (Status is not PaymentStatus.Paid) return;
        Status = PaymentStatus.Refunded;
    }
}
