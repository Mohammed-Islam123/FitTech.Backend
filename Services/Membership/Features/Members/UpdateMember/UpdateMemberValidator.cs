using FluentValidation;

namespace Membership.Features.Members.UpdateMember;

public class UpdateMemberValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Request.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty();

        RuleFor(x => x.Request.NewPassword)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Request.OldPassword))
            .WithMessage("New password is required when changing password.");

        RuleFor(x => x.Request.OldPassword)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Request.NewPassword))
            .WithMessage("Old password is required when changing password.");
    }
}
