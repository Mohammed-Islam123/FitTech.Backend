namespace Membership.Domain.Entities;

/// <description>
/// Represents a payment approval request from a member.
/// Used for offline/cash payment workflows where an admin must approve
/// before the payment is recorded and the service is activated.
/// </description>
public class PaymentApprovalRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid MemberId { get; set; }
    public string RequestType { get; set; } = null!; // "MembershipRenewal", "CoursePurchase"
    public decimal Amount { get; set; }
    public Guid ReferenceId { get; set; } // SubscriptionId or CourseId
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public static class PaymentApprovalRequestType
{
    public const string MembershipRenewal = "MembershipRenewal";
    public const string CoursePurchase = "CoursePurchase";
}

public static class PaymentApprovalRequestStatus
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
}
