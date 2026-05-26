using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Domain.Entities;
using Payment.Infrastructure;
using Payment.Infrastructure.Persistence;

namespace Payment.Features.Requests.ListMembershipRenewalRequests;

public class ListMembershipRenewalRequestsHandler(
    PaymentDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListRequestResponse>>> Handle(
        ListMembershipRenewalRequestsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can view requests.");

        var requests = await context.PaymentRequests
            .AsNoTracking()
            .Where(r => r.RequestType == PaymentRequestType.MembershipRenewal && r.Status == PaymentRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var results = new List<ListRequestResponse>();
        foreach (var r in requests)
        {
            var profile = await identityClient.GetProfileAsync(r.MemberId);
            var name = profile.IsSuccessStatusCode
                ? $"{profile.Content?.Data?.FirstName} {profile.Content?.Data?.LastName}".Trim()
                : "Unknown";
            var email = profile.IsSuccessStatusCode ? profile.Content?.Data?.Email ?? "" : "";

            results.Add(new ListRequestResponse(r.Id, r.MemberId, name, email, r.Amount, r.ReferenceId, r.Status.ToString(), r.CreatedAt));
        }
        return results;
    }
}
