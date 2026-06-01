using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.ListCoaches;

/// <description>
/// Returns a paginated list of all coaches with admin-level detail.
/// Accessible to Admin role only.
/// </description>
public class ListCoachesHandler(CoursesDbContext context)
{
    public async Task<ErrorOr<ListCoachesResponse>> Handle(
        ListCoachesQuery query,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        var totalCount = await context.Coaches
            .AsNoTracking()
            .CountAsync(ct);

        var coaches = await context.Coaches
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CoachSummaryDto(
                CoachId: c.Id,
                FirstName: c.FirstName,
                LastName: c.LastName,
                Email: c.Email,
                PhoneNumber: c.PhoneNumber,
                Bio: c.Bio,
                Specialties: c.Specialties,
                ProfilePhotoUrl: c.ProfilePhotoUrl,
                CreatedAt: c.CreatedAt
            ))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ListCoachesResponse(
            coaches,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }
}
