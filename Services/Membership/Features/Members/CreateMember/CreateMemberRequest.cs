using Membership.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Membership.Features.Members.CreateMember;

public class CreateMemberRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public Guid PlanId { get; set; }
    public string? CardUid { get; set; }

    public IFormFile? MedicalCertificate { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}
