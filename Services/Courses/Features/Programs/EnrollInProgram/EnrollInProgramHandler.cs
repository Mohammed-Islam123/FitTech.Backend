using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.EnrollInProgram;

/// <description>
/// Enrolls the authenticated member in a program. Validates that the program
/// is in Accepted status and the member is not already enrolled.
/// </description>
public class EnrollInProgramHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<EnrollInProgramResponse>> Handle(
        EnrollInProgramCommand command,
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
        {
            return Error.Unauthorized(
                "Enrollment.Unauthorized",
                "Authentication required to enroll in a program.");
        }

        if (!userAccessor.IsMember)
        {
            return Error.Unauthorized(
                "Enrollment.NotMember",
                "Only members can enroll in programs.");
        }

        var program = await context.Programs
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, ct);

        if (program is null)
        {
            return Error.NotFound(
                "Program.NotFound",
                "The specified program does not exist.");
        }

        if (program.Status != ProgramStatus.Accepted)
        {
            return Error.Validation(
                "Program.NotAccepted",
                "You can only enroll in programs that have been accepted.");
        }

        var alreadyEnrolled = program.Enrollments
            .Any(e => e.MemberId == currentUserId);

        if (alreadyEnrolled)
        {
            return Error.Conflict(
                "Enrollment.AlreadyEnrolled",
                "You are already enrolled in this program.");
        }

        var enrollment = new ProgramEnrollment
        {
            ProgramId = command.ProgramId,
            MemberId = currentUserId.Value,
            EnrolledAt = DateTime.UtcNow
        };

        context.ProgramEnrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        return new EnrollInProgramResponse(
            enrollment.Id,
            enrollment.ProgramId,
            enrollment.MemberId,
            enrollment.EnrolledAt);
    }
}
