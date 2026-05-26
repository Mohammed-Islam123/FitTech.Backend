using Courses.Domain.Enums;

namespace Courses.Domain.Entities;

/// <description>
/// Records attendance for a member at a specific session.
/// </description>
public class AttendanceRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid SessionId { get; set; }
    public Guid MemberId { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    public Session Session { get; set; } = null!;
}
