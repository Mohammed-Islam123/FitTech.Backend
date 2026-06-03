using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetMyPurchaseRequests;

public class GetMyPurchaseRequestsHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetMyPurchaseRequestsResponse>>> Handle(
        GetMyPurchaseRequestsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view purchase requests.");
        }

        if (userAccessor.UserId is null)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "User identity not found in token.");
        }

        var coach = await context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userAccessor.UserId.Value, ct);

        if (coach is null)
        {
            return Error.NotFound("Coach.NotFound", "Coach not found.");
        }

        return await context.CoursePurchaseRequests
            .AsNoTracking()
            .Include(r => r.Program)
            .Where(r => r.Program.CoachId == coach.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetMyPurchaseRequestsResponse(
                r.Id,
                r.ProgramId,
                r.Program.Name,
                r.MemberId,
                r.Amount,
                r.PaymentMethod,
                r.Status,
                r.Notes,
                r.CreatedAt,
                r.ResolvedAt))
            .ToListAsync(ct);
    }
}
