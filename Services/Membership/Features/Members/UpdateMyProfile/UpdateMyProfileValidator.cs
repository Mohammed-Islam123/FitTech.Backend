using FluentValidation;

namespace Membership.Features.Members.UpdateMyProfile;

/// <description>
/// Validates password-change rules only.
/// No other fields are required — PATCH semantics allow partial updates.
/// </description>
public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileValidator()
    {
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
