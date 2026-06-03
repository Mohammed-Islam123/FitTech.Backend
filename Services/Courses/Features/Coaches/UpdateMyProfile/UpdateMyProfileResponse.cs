namespace Courses.Features.Coaches.UpdateMyProfile;

public record UpdateMyProfileResponse(
    Guid CoachId,
    string? Bio,
    string? Specialties,
    string? ProfilePhotoUrl
);
