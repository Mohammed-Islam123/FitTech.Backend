using Courses.Domain.Enums;

namespace Courses.Features.Coaches.CreateCoach;

public record CreateCoachRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    Gender? Gender,
    string? Bio
);
