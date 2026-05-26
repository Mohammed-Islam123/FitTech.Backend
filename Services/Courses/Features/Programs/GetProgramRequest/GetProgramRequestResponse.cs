namespace Courses.Features.Programs.GetProgramRequest;

public record GetProgramRequestResponse(
    Guid ProgramId,
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
    string Status,
    string CoachName,
    List<TimeSlotResponse> TimeSlots
);

public record TimeSlotResponse(
    Guid Id,
    string Day,
    string StartTime,
    string EndTime,
    string? Description
);
