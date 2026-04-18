using Membership.Domain.Enums;

namespace Membership.Features.Plans.ListPlans;

public record ListPlansResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int? DurationValue,
    DurationUnit? DurationUnit,
    int? SessionCount,
    string[]? AccessRules,
    bool IsActive
);
