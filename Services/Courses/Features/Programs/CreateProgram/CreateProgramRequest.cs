namespace Courses.Features.Programs.CreateProgram;

public record CreateProgramRequest(
    string Name,
    string? Description,
    string? Level,
    string? ExerciseType,
    int DurationMinutes,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalPrice,
    int MaxParticipants,
    string? PictureUrl,
    List<TimeSlotRequest> TimeSlots
);

public record TimeSlotRequest(
    string Day,
    string StartTime,
    string EndTime,
    string? Description
);
