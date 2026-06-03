using FluentValidation;

namespace Courses.Features.Coaches.UpdateMyProfile;

public class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.Request.Bio)
            .MaximumLength(2000)
            .When(x => x.Request.Bio is not null);

        RuleFor(x => x.Request.Specialties)
            .MaximumLength(500)
            .When(x => x.Request.Specialties is not null);

        RuleFor(x => x.Request.ProfilePhotoUrl)
            .MaximumLength(500)
            .When(x => x.Request.ProfilePhotoUrl is not null);
    }
}
