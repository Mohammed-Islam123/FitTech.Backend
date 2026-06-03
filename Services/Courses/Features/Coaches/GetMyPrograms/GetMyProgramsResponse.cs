namespace Courses.Features.Coaches.GetMyPrograms;

public record GetMyProgramsResponse(
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
