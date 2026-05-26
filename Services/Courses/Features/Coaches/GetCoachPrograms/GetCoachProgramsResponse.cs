namespace Courses.Features.Coaches.GetCoachPrograms;

public record GetCoachProgramsResponse(
    Guid ProgramId,
    string Name,
    string? Description,
    string? Level,
    string? ExerciseType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalPrice,
    string Status,
    int EnrolledCount
);
