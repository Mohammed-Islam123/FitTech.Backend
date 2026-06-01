using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.ListPurchaseRequests;

/// <description>
/// Returns all pending cash course purchase requests for admin review.
/// Admin-only access.
/// </description>
public class ListPurchaseRequestsHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListPurchaseRequestsResponse>>> Handle(
        ListPurchaseRequestsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Purchase.Unauthorized", "Only Administrators can view purchase requests.");

        return await context.CoursePurchaseRequests
            .AsNoTracking()
            .Where(r => r.PaymentMethod == "Cash" && r.Status == CoursePurchaseRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ListPurchaseRequestsResponse(
                r.Id,
                r.MemberId,
                r.ProgramId,
                r.Program.Name,
                r.Program.Coach.FirstName + " " + r.Program.Coach.LastName,
                r.Amount,
                r.PaymentMethod,
                r.Status,
                r.CreatedAt,
                r.Notes))
            .ToListAsync(ct);
    }
}
