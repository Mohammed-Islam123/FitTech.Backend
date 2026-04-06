namespace Membership.Domain.Entities;

public class MemberHealthProfile
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string? Objectives { get; set; }
    public string? MedicalRestrictions { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    public Member Member { get; set; } = null!;
}

