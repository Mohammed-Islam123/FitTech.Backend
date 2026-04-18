using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.ListMembers;

public class ListMembersHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<ListMembersResponse>> Handle(
        ListMembersQuery query,
        CancellationToken ct)
    {
        // Authorization: Only Admin or Coach can list members
        if (!userAccessor.IsAdmin && !userAccessor.IsCoach)
        {
            return Error.Forbidden("Member.Forbidden", "You are not authorized to list members.");
        }

        var req = query.Request;
        var baseQuery = context.Members.AsNoTracking();

        // Apply Search Filter
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var searchTerm = $"%{req.Search}%";
            baseQuery = baseQuery.Where(m => 
                EF.Functions.ILike(m.FirstName, searchTerm) || 
                EF.Functions.ILike(m.LastName, searchTerm));
        }

        // Apply Status Filter
        if (req.Status.HasValue)
        {
            baseQuery = baseQuery.Where(m => m.Status == req.Status.Value);
        }

        // Total Count
        var totalCount = await baseQuery.CountAsync(ct);

        // Sorting & Pagination
        var items = await baseQuery
            .OrderByDescending(m => m.JoinDate)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(m => new MemberSummaryDto(
                m.Id,
                m.FirstName,
                m.LastName,
                m.Status,
                m.JoinDate,
                m.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .OrderByDescending(s => s.StartOnUTC)
                    .Select(s => s.Plan.Name)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize);

        return new ListMembersResponse(
            items,
            totalCount,
            req.Page,
            req.PageSize,
            totalPages);
    }
}
