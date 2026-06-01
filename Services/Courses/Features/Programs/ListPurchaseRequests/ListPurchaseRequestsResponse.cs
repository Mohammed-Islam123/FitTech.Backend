namespace Courses.Features.Programs.ListPurchaseRequests;

public record ListPurchaseRequestsResponse(
    Guid RequestId,
    Guid MemberId,
    Guid ProgramId,
    string ProgramName,
    string CoachName,
    decimal Amount,
    string PaymentMethod,
    string Status,
    DateTime CreatedAt,
    string? Notes
);
