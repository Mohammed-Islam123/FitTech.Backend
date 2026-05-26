using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.GetProgramMembers;

public class GetProgramMembersHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetProgramMembersResponse>>> Handle(
        GetProgramMembersQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Only Coaches and Administrators can view program members.");
        }

        return await context.ProgramEnrollments
            .AsNoTracking()
            .Where(e => e.ProgramId == query.ProgramId)
            .Select(e => new GetProgramMembersResponse(
                e.MemberId,
                $"Member-{e.MemberId.ToString().Substring(0, 8)}",
                string.Empty,
                e.EnrolledAt))
            .ToListAsync(ct);
    }
}
