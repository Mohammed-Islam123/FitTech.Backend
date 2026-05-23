using Shared.Enums;

namespace Membership.Features.Subscriptions.ConfirmCashPayment;

public record ConfirmCashPaymentRequest(
    Guid SubscriptionId,
    decimal AmountReceived,
    PaymentMethod PaymentMethod,
    string? Notes);
