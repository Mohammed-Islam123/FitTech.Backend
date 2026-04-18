using Membership.Domain.Enums;

namespace Membership.Features.Plans.CreatePlan;

public record CreatePlanRequest(
    string Name,
    string? Description,
    decimal Price,
    int? DurationValue,
    DurationUnit? DurationUnit,
    int? SessionCount,
    string[]? AccessRules
);
