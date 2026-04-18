using FluentValidation;
using Membership.Domain.Enums;

namespace Membership.Features.Members.CreateMember;

public class CreateMemberValidator : AbstractValidator<CreateMemberRequest>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty();

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-16))
            .WithMessage("Member must be at least 16 years old.");

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.PlanId)
            .NotEmpty();

        RuleFor(x => x.CardUid)
            .NotEmpty()
            .When(x => x.CardUid != null);
    }
}
