namespace Courses.Features.Programs.PurchaseOnline;

public record PurchaseOnlineResponse(
    Guid EnrollmentId,
    Guid ProgramId,
    string ProgramName,
    Guid? PaymentId,
    decimal Amount,
    string PaymentMethod,
    DateTime PurchasedAt
);
