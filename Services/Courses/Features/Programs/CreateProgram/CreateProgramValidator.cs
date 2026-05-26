using FluentValidation;

namespace Courses.Features.Programs.CreateProgram;

public class CreateProgramValidator : AbstractValidator<CreateProgramCommand>
{
    public CreateProgramValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
        RuleFor(x => x.Request.Level).MaximumLength(50);
        RuleFor(x => x.Request.ExerciseType).MaximumLength(100);
        RuleFor(x => x.Request.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.Request.StartDate).NotEmpty();
        RuleFor(x => x.Request.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.Request.StartDate);
        RuleFor(x => x.Request.TotalPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MaxParticipants).GreaterThan(0);
        RuleFor(x => x.Request.TimeSlots).NotEmpty();
        RuleForEach(x => x.Request.TimeSlots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.Day).NotEmpty();
            slot.RuleFor(s => s.StartTime).NotEmpty();
            slot.RuleFor(s => s.EndTime).NotEmpty();
        });
    }
}
