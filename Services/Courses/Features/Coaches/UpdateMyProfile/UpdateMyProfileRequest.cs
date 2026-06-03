namespace Courses.Features.Coaches.UpdateMyProfile;

public record UpdateMyProfileRequest(
    string? Bio,
    string? Specialties,
    string? ProfilePhotoUrl
);
