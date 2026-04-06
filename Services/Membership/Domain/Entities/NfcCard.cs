namespace Membership.Domain.Entities;

public class NfcCard
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string CardUid { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    public Member Member { get; set; } = null!;
}

