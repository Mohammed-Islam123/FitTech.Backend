using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;

namespace Membership.Features.Subscriptions.RequestRenewal;

/// <description>
/// Member submits a renewal request. Creates a PaymentApprovalRequest
/// with status Pending for admin review.
/// </description>
public class RequestRenewalHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<RequestRenewalResponse>> Handle(
        RequestRenewalCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Request.Unauthorized", "Only Members can submit renewal requests.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Request.Unauthorized", "Authentication required.");

        var req = command.Request;

        var request = new PaymentApprovalRequest
        {
            Id = Guid.CreateVersion7(),
            MemberId = userId.Value,
            RequestType = PaymentApprovalRequestType.MembershipRenewal,
            Amount = req.Amount,
            ReferenceId = req.SubscriptionId,
            Notes = req.Notes,
            Status = PaymentApprovalRequestStatus.Pending
        };

        context.PaymentApprovalRequests.Add(request);
        await context.SaveChangesAsync(ct);

        return new RequestRenewalResponse(request.Id);
    }
}
