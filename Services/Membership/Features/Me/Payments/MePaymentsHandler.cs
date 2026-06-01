using ErrorOr;
using Membership.Common.Security;
using Membership.Infrastructure;

namespace Membership.Features.Me.Payments;

/// <description>
/// Returns the authenticated member's payment history by proxying to the Payment service.
/// </description>
public class MePaymentsHandler(
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient)
{
    public async Task<ErrorOr<List<MePaymentResponse>>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Me.Unauthorized", "Only Members can view their payment history.");

        var response = await paymentClient.GetMyPaymentsAsync();
        if (!response.IsSuccessStatusCode || response.Content is null)
            return Error.Failure("Payment.Failed", "Failed to retrieve payment history from Payment service.");

        return response.Content.Select(p => new MePaymentResponse(
            p.PaymentId,
            p.Amount,
            p.PaymentMethod,
            p.PaymentType,
            p.ReferenceId,
            p.Status,
            p.CreatedAt
        )).ToList();
    }
}
