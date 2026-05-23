using FluentValidation;
using Shared.Enums;

namespace Membership.Features.Subscriptions.ConfirmCashPayment;

public class ConfirmCashPaymentValidator : AbstractValidator<ConfirmCashPaymentRequest>
{
    public ConfirmCashPaymentValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("SubscriptionId is required.");

        RuleFor(x => x.AmountReceived)
            .GreaterThan(0).WithMessage("AmountReceived must be greater than 0.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("PaymentMethod must be a valid enum value.");
    }
}
