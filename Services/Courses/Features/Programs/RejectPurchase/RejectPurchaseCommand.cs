namespace Courses.Features.Programs.RejectPurchase;

public record RejectPurchaseCommand(Guid RequestId, string? Reason = null);
