namespace Courses.Features.Programs.PurchaseCash;

public record PurchaseCashCommand(Guid ProgramId, PurchaseCashRequest Request);
