using FluentValidation;

namespace Courses.Features.Sessions.GetSessionHistory;

public class GetSessionHistoryValidator : AbstractValidator<GetSessionHistoryQuery>
{
    public GetSessionHistoryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");

        RuleFor(x => x.EndDate)
            .Must((query, endDate) =>
            {
                var twelveMonthsAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
                return endDate >= twelveMonthsAgo;
            })
            .WithMessage("End date cannot be more than 12 months in the past.");
    }
}
