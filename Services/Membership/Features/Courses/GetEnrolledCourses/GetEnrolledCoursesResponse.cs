namespace Membership.Features.Courses.GetEnrolledCourses;

public record GetEnrolledCoursesResponse(
    Guid Id,
    string Name,
    string? CoachName,
    string? Description
);
