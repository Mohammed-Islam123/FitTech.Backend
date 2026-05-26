namespace Payment.Features.Requests.ListMembershipRenewalRequests;

public record ListRequestResponse(
    Guid RequestId,
    Guid MemberId,
    string MemberName,
    string Email,
    decimal Amount,
    Guid ReferenceId,
    string Status,
    DateTime CreatedAt
);
