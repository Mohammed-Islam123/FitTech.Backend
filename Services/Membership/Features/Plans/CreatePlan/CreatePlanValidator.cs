using FluentValidation;

namespace Membership.Features.Plans.CreatePlan;

public class CreatePlanValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Request.DurationValue)
            .GreaterThan(0)
            .When(x => x.Request.DurationValue.HasValue);

        RuleFor(x => x.Request.DurationUnit)
            .NotNull()
            .When(x => x.Request.DurationValue.HasValue);
    }
}
