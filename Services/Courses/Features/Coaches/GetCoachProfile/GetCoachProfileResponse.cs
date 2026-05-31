namespace Courses.Features.Coaches.GetCoachProfile;

/// <description>
/// Public profile of a coach without sensitive contact information.
/// </description>
public record GetCoachProfileResponse(
    Guid CoachId,
    string FirstName,
    string LastName,
    string? Bio,
    string? Specialties,
    string? ProfilePhotoUrl
);
