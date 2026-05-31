namespace Courses.Features.Programs.GetProgramDetail;

/// <description>
/// Full details of a program including time slots and enrollment info.
/// </description>
public record GetProgramDetailResponse(
    Guid Id,
    string Name,
    decimal Price,
    string? Description,
    int SpotsLeft,
    int Capacity,
    string? CoachName,
    string? Level,
    string? ExerciseType,
    int DurationMinutes,
    List<ProgramTimeSlotResponse> TimeSlots
);

public record ProgramTimeSlotResponse(
    Guid Id,
    string Day,
    string StartTime,
    string EndTime,
    string? Description
);
