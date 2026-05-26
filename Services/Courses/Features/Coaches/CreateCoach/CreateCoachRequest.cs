namespace Courses.Features.Coaches.CreateCoach;

public record CreateCoachRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    Guid UserId,
    string? Bio
);
