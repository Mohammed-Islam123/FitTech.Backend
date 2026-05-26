namespace Courses.Domain.Entities;

/// <description>
/// Represents a single session instance generated after program acceptance.
/// </description>
public class Session
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProgramId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsCompleted { get; set; }

    public Program Program { get; set; } = null!;
    public ProgramTimeSlot TimeSlot { get; set; } = null!;
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
}
