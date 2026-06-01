namespace Courses.Features.Programs.PurchaseOnline;

public record PurchaseOnlineCommand(Guid ProgramId, PurchaseOnlineRequest Request);
