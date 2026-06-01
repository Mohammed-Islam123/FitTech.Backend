namespace Courses.Features.Programs.AcceptPurchase;

public record AcceptPurchaseCommand(Guid RequestId, string? Notes = null);
