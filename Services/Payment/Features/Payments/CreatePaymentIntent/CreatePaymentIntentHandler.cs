using ErrorOr;
using Payment.Infrastructure.Gateways;
using Payment.Infrastructure.Persistence;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Features.Payments.CreatePaymentIntent;

public class CreatePaymentIntentHandler(
    PaymentDbContext context,
    IPaymentGateway gateway)
{
    public async Task<ErrorOr<CreatePaymentIntentResponse>> Handle(
        CreatePaymentIntentCommand command,
        CancellationToken ct)
    {
        var req = command.Request;

        var payment = PaymentEntity.Create(
            userId: req.UserId,
            amount: req.Amount,
            paymentMethod: req.PaymentMethod,
            paymentType: req.PaymentType,
            referenceId: req.ReferenceId,
            notes: req.Notes);

        // Payment starts Pending for online/credit card (determined in Create factory)

        var intent = await gateway.CreateIntentAsync(
            req.Amount, "DZD", payment.Id, ct);

        payment.GatewayTransactionId = intent.GatewayTransactionId;

        context.Payments.Add(payment);
        await context.SaveChangesAsync(ct);

        return new CreatePaymentIntentResponse(
            payment.Id, intent.ClientSecret, intent.GatewayTransactionId);
    }
}
