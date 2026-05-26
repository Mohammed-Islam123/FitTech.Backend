namespace Activity.Domain.Entities;

public class MemberActivity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid MemberId { get; set; }
    public string? CardUid { get; set; }
    public Guid? CourseId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public bool IsManual { get; set; }
}
