namespace Payment.Domain.Entities;

/// <description>
/// Represents an offline payment request (membership renewal or course purchase) awaiting admin approval.
/// </description>
public class PaymentRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid MemberId { get; set; }
    public PaymentRequestType RequestType { get; set; }
    public decimal Amount { get; set; }
    public Guid ReferenceId { get; set; }
    public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public enum PaymentRequestType
{
    MembershipRenewal,
    CoursePurchase
}

public enum PaymentRequestStatus
{
    Pending,
    Accepted,
    Rejected
}
