namespace Courses.Features.Programs.EnrollInProgram;

/// <description>
/// Response returned when a member successfully enrolls in a program.
/// </description>
public record EnrollInProgramResponse(
    Guid EnrollmentId,
    Guid ProgramId,
    Guid MemberId,
    DateTime EnrolledAt
);
