using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;
using Shared.Events;
using Wolverine;

namespace Payment.Features.Requests.AcceptCoursePurchase;

public class AcceptCoursePurchaseHandler(
    PaymentDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<AcceptRequestResponse>> Handle(
        AcceptCoursePurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can accept requests.");

        var request = await context.PaymentRequests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Request not found.");

        if (request.Status != PaymentRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "Request has already been resolved.");

        request.Status = PaymentRequestStatus.Accepted;
        request.ResolvedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new CoursePurchaseAcceptedEvent(
            request.Id, request.MemberId, request.Amount, request.ReferenceId, request.ResolvedAt!.Value));

        return new AcceptRequestResponse(request.Id, request.Status.ToString());
    }
}
