namespace Shared.Events;

public record CoursePurchaseAcceptedEvent(
    Guid RequestId,
    Guid MemberId,
    decimal Amount,
    Guid CourseId,
    DateTime AcceptedAt
);
