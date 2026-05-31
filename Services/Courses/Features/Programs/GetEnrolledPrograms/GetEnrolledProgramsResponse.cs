namespace Courses.Features.Programs.GetEnrolledPrograms;

/// <description>
/// Summary of a program the authenticated member is enrolled in.
/// </description>
public record GetEnrolledProgramsResponse(
    Guid Id,
    string Name,
    string? CoachName,
    string? Description,
    DateTime EnrolledAt
);
