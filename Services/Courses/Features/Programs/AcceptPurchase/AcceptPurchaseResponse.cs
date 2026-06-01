namespace Courses.Features.Programs.AcceptPurchase;

public record AcceptPurchaseResponse(Guid RequestId, Guid EnrollmentId, string Status);
