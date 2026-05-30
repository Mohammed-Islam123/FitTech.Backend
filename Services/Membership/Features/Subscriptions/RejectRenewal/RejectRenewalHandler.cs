using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Membership.Features.Subscriptions.RejectRenewal;

/// <description>
/// Admin rejects a renewal request. Updates request status and notifies the member.
/// </description>
public class RejectRenewalHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<RejectRenewalResponse>> Handle(
        RejectRenewalCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can reject requests.");

        var req = command.Request;

        var request = await context.PaymentApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == req.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Renewal request not found.");

        if (request.Status != PaymentApprovalRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "This request has already been resolved.");

        request.Status = PaymentApprovalRequestStatus.Rejected;
        request.ResolvedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        // Publish notification so the member knows their request was rejected
        await messageBus.PublishAsync(new SendEmailEvent(
            To: "placeholder@email.com", // resolved from Identity in production
            Subject: "Renewal Request Rejected",
            Body: req.Reason is not null
                ? $"Your renewal request has been rejected. Reason: {req.Reason}"
                : "Your renewal request has been rejected. Contact the gym for details."
        ));

        return new RejectRenewalResponse(request.Id, request.Status);
    }
}
