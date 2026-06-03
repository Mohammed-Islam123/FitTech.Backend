namespace Courses.Features.Coaches.GetMySessions;

public record GetMySessionsResponse(
    Guid SessionId,
    Guid ProgramId,
    string ProgramName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsCompleted,
    string TimeSlotDay,
    string? TimeSlotDescription
);
