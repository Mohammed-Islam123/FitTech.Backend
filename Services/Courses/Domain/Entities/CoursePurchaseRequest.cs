namespace Courses.Domain.Entities;

/// <description>
/// Represents a course purchase request from a member.
/// Used for both cash (admin approval) and online (auto-processed) payment flows.
/// </description>
public class CoursePurchaseRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid MemberId { get; set; }
    public Guid ProgramId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!; // "Cash" or "CreditCard"
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public Program Program { get; set; } = null!;
}

public static class CoursePurchaseRequestStatus
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
}
