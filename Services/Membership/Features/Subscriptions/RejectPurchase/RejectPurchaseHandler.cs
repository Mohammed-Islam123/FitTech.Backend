using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Membership.Features.Subscriptions.RejectPurchase;

/// <description>
/// Admin rejects a purchase request. Updates request status and notifies the member.
/// </description>
public class RejectPurchaseHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<RejectPurchaseResponse>> Handle(
        RejectPurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can reject requests.");

        var req = command.Request;

        var request = await context.PaymentApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == req.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Purchase request not found.");

        if (request.RequestType != PaymentApprovalRequestType.PlanPurchase)
            return Error.Validation("Request.InvalidType", "This request is not a plan purchase request.");

        if (request.Status != PaymentApprovalRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "This request has already been resolved.");

        request.Status = PaymentApprovalRequestStatus.Rejected;
        request.ResolvedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new SendEmailEvent(
            To: "placeholder@email.com",
            Subject: "Purchase Request Rejected",
            Body: req.Reason is not null
                ? $"Your plan purchase request has been rejected. Reason: {req.Reason}"
                : "Your plan purchase request has been rejected. Contact the gym for details."
        ));

        return new RejectPurchaseResponse(request.Id, request.Status);
    }
}
