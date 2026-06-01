namespace Courses.Features.Programs.PurchaseCash;

public record PurchaseCashRequest(
    decimal Amount,
    string? Notes = null
);
