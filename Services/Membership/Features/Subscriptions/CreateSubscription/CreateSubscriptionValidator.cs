using FluentValidation;

namespace Membership.Features.Subscriptions.CreateSubscription;

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionRequest>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("MemberId is required.");

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("PlanId is required.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("PaymentMethod must be a valid enum value.");
    }
}
