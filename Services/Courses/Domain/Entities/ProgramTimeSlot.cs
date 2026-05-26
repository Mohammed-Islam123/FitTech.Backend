using Courses.Domain.Enums;

namespace Courses.Domain.Entities;

/// <description>
/// Defines a recurring time slot for a program (e.g., "Monday 08:00-09:00").
/// </description>
public class ProgramTimeSlot
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProgramId { get; set; }
    public CourseDayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Description { get; set; }

    public Program Program { get; set; } = null!;
    public ICollection<Session> Sessions { get; set; } = [];
}
