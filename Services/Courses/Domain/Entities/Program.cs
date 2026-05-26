using Courses.Domain.Enums;

namespace Courses.Domain.Entities;

/// <description>
/// Represents a training program/course created by a coach.
/// </description>
public class Program
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CoachId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Level { get; set; }
    public string? ExerciseType { get; set; }
    public int DurationMinutes { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public int MaxParticipants { get; set; }
    public string? PictureUrl { get; set; }
    public ProgramStatus Status { get; set; } = ProgramStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Coach Coach { get; set; } = null!;
    public ICollection<ProgramTimeSlot> TimeSlots { get; set; } = [];
    public ICollection<ProgramEnrollment> Enrollments { get; set; } = [];
    public ICollection<Session> Sessions { get; set; } = [];
}
