namespace Courses.Features.Coaches.ListCoaches;

/// <description>
/// Paginated list of coaches with admin-facing profile details.
/// </description>
public record ListCoachesResponse(
    List<CoachSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <description>
/// Coach summary with admin-level detail including contact info.
/// </description>
public record CoachSummaryDto(
    Guid CoachId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? Bio,
    string? Specialties,
    string? ProfilePhotoUrl,
    DateTime CreatedAt
);
