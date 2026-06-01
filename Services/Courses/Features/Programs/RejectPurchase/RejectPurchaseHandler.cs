using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Courses.Features.Programs.RejectPurchase;

/// <description>
/// Admin rejects a course purchase request. Notifies the member.
/// </description>
public class RejectPurchaseHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<RejectPurchaseResponse>> Handle(
        RejectPurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Purchase.Unauthorized", "Only Administrators can reject purchase requests.");

        var request = await context.CoursePurchaseRequests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Purchase request not found.");

        if (request.Status != CoursePurchaseRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "This request has already been resolved.");

        request.Status = CoursePurchaseRequestStatus.Rejected;
        request.ResolvedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new SendEmailEvent(
            To: $"member-{request.MemberId}",
            Subject: "Purchase Request Rejected",
            Body: command.Reason is not null
                ? $"Your program purchase request has been rejected. Reason: {command.Reason}"
                : "Your program purchase request has been rejected. Contact the gym for details."
        ));

        return new RejectPurchaseResponse(request.Id, request.Status);
    }
}
