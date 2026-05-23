using FluentValidation;
using Shared.Enums;

namespace Payment.Features.Payments.CreatePayment;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("PaymentMethod must be a valid enum value.");

        RuleFor(x => x.PaymentType)
            .IsInEnum().WithMessage("PaymentType must be a valid enum value.");

        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required.");
    }
}
