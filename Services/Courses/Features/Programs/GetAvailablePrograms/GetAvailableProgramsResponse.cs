namespace Courses.Features.Programs.GetAvailablePrograms;

/// <description>
/// Summary of a program that the member is not yet enrolled in.
/// </description>
public record GetAvailableProgramsResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    string? CoachName,
    string? Description
);
