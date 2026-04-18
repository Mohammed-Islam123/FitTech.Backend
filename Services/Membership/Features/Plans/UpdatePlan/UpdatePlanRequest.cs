using Membership.Domain.Enums;

namespace Membership.Features.Plans.UpdatePlan;

public record UpdatePlanRequest(
    string Name,
    string? Description,
    decimal Price,
    int? DurationValue,
    DurationUnit? DurationUnit,
    int? SessionCount,
    string[]? AccessRules,
    bool IsActive
);
