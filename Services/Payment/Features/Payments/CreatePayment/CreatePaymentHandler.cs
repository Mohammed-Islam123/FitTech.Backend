using ErrorOr;
using PaymentEntity = Payment.Domain.Entities.Payment;
using Payment.Infrastructure.Persistence;
using Shared.Events;
using Wolverine;

namespace Payment.Features.Payments.CreatePayment;

public class CreatePaymentHandler(
    PaymentDbContext context,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<CreatePaymentResponse>> Handle(
        CreatePaymentCommand command,
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

        context.Payments.Add(payment);
        await context.SaveChangesAsync(ct);

        var confirmedEvent = new PaymentConfirmedEvent(
            PaymentId: payment.Id,
            UserId: payment.UserId,
            Amount: payment.Amount,
            Currency: payment.Currency,
            PaymentMethod: payment.PaymentMethod,
            PaymentType: payment.PaymentType,
            ReferenceId: payment.ReferenceId,
            CreatedAt: payment.CreatedAt);

        await messageBus.PublishAsync(confirmedEvent);

        return new CreatePaymentResponse(payment.Id);
    }
}
