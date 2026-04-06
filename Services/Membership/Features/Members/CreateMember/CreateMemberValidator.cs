using FluentValidation;
using Membership.Domain;

namespace Membership.Features.Members.CreateMember;

internal class CreateMemberValidator : AbstractValidator<CreateMemberRequest>
{
    internal CreateMemberValidator(MembershipDbContext context)
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

        RuleFor(x => x.CardUid)
            .Must(uid => !context.NfcCards.Any(c => c.CardUid == uid))
            .When(x => !string.IsNullOrEmpty(x.CardUid))
            .WithMessage("The provided NFC card is already assigned to another member.");
    }
}
