namespace Payment.Features.Requests.RequestCoursePurchase;

public record RequestCoursePurchaseRequest(
    Guid CourseId,
    decimal Amount,
    string? Notes
);
