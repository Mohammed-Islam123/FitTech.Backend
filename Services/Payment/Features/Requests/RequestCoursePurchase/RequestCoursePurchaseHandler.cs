using ErrorOr;
using Payment.Common.Security;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;

namespace Payment.Features.Requests.RequestCoursePurchase;

public class RequestCoursePurchaseHandler(
    PaymentDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<RequestCoursePurchaseResponse>> Handle(
        RequestCoursePurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
        {
            return Error.Unauthorized(
                "Request.Unauthorized",
                "Only Members can submit payment requests.");
        }

        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
            return Error.Unauthorized("Request.Unauthorized", "Authentication required.");

        var request = new PaymentRequest
        {
            Id = Guid.CreateVersion7(),
            MemberId = currentUserId.Value,
            RequestType = PaymentRequestType.CoursePurchase,
            Amount = command.Request.Amount,
            ReferenceId = command.Request.CourseId,
            Notes = command.Request.Notes,
            Status = PaymentRequestStatus.Pending
        };

        context.PaymentRequests.Add(request);
        await context.SaveChangesAsync(ct);

        return new RequestCoursePurchaseResponse(request.Id);
    }
}
