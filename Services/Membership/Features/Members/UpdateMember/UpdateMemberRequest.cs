using Membership.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Membership.Features.Members.UpdateMember;

public record UpdateMemberRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    Gender? Gender,
    DateOnly? DateOfBirth,
    IFormFile? ProfilePicture,
    string? Objectives,
    string? MedicalRestrictions,
    MemberStatus? Status = null, // Admin only
    string? OldPassword = null, // Self-update only
    string? NewPassword = null // Self-update only
);
