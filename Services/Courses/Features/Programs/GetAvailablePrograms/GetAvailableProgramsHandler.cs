using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.GetAvailablePrograms;

/// <description>
/// Returns accepted programs the authenticated member is NOT enrolled in.
/// </description>
public class GetAvailableProgramsHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetAvailableProgramsResponse>>> Handle(
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
        {
            return Error.Unauthorized(
                "Programs.Unauthorized",
                "Authentication required.");
        }

        var enrolledProgramIds = await context.ProgramEnrollments
            .AsNoTracking()
            .Where(e => e.MemberId == currentUserId)
            .Select(e => e.ProgramId)
            .ToListAsync(ct);

        return await context.Programs
            .AsNoTracking()
            .Where(p => p.Status == ProgramStatus.Accepted
                        && !enrolledProgramIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .Select(p => new GetAvailableProgramsResponse(
                p.Id,
                p.Name,
                p.PictureUrl,
                p.TotalPrice,
                p.CoachId,
                p.Coach.FirstName + " " + p.Coach.LastName,
                p.Description))
            .ToListAsync(ct);
    }
}
