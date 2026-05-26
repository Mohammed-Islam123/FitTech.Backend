namespace Courses.Domain.Entities;

/// <description>
/// Tracks member enrollment in a program.
/// </description>
public class ProgramEnrollment
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProgramId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public Program Program { get; set; } = null!;
}
