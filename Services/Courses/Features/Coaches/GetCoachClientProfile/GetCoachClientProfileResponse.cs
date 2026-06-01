namespace Courses.Features.Coaches.GetCoachClientProfile;

public record GetCoachClientProfileResponse(
    Guid MemberId,
    Guid UserId,
    string FullName,
    string? Email,
    string? PhoneNumber,
    DateTime JoinDate,
    string? MedicalFileUrl
);
