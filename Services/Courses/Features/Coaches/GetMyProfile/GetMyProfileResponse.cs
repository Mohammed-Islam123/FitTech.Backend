namespace Courses.Features.Coaches.GetMyProfile;

public record GetMyProfileResponse(
    Guid CoachId,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? Bio,
    string? Specialties,
    string? ProfilePhotoUrl,
    DateTime CreatedAt
);
