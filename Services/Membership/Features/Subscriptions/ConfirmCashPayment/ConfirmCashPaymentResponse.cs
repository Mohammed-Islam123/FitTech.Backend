using Shared.Enums;

namespace Membership.Features.Subscriptions.ConfirmCashPayment;

public record ConfirmCashPaymentResponse(
    Guid SubscriptionId,
    Guid PaymentId,
    PaymentStatus PaymentStatus);
