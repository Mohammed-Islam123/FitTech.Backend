using FluentValidation;

namespace Courses.Features.Coaches.CreateCoach;

public class CreateCoachValidator : AbstractValidator<CreateCoachCommand>
{
    public CreateCoachValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Request.PhoneNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Bio).MaximumLength(2000);
    }
}
