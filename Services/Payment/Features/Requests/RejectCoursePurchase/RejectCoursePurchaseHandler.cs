using Carter;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;
using Payment.Shared;
using Wolverine;

namespace Payment.Features.Requests.RejectCoursePurchase;

public class RejectCoursePurchaseHandler(
    PaymentDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<AcceptRequestResponse>> Handle(
        RejectCoursePurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can reject requests.");

        var request = await context.PaymentRequests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Request not found.");

        if (request.Status != PaymentRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "Request has already been resolved.");

        request.Status = PaymentRequestStatus.Rejected;
        request.ResolvedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return new AcceptRequestResponse(request.Id, request.Status.ToString());
    }
}
