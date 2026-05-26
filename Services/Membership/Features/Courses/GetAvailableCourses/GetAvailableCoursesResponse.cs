namespace Membership.Features.Courses.GetAvailableCourses;

public record GetAvailableCoursesResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    string? CoachName
);
