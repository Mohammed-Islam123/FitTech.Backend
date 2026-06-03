namespace Courses.Features.Coaches.GetMyPurchaseRequests;

public record GetMyPurchaseRequestsResponse(
    Guid RequestId,
    Guid ProgramId,
    string ProgramName,
    Guid MemberId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);
