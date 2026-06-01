namespace Courses.Features.Programs.PurchaseOnline;

public record PurchaseOnlineRequest(
    decimal Amount,
    string? Notes = null
);
