using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.ListProgramRequests;

public class ListProgramRequestsHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListProgramRequestsResponse>>> Handle(
        ListProgramRequestsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Program.Unauthorized",
                "Only Administrators can review program requests.");
        }

        return await context.Programs
            .AsNoTracking()
            .Where(p => p.Status == ProgramStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ListProgramRequestsResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Coach.FirstName + " " + p.Coach.LastName))
            .ToListAsync(ct);
    }
}
