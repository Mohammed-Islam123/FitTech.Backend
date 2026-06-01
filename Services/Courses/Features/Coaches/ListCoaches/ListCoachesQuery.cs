namespace Courses.Features.Coaches.ListCoaches;

/// <description>
/// Query parameters for listing coaches with pagination.
/// </description>
public record ListCoachesQuery(
    int Page = 1,
    int PageSize = 10
);
