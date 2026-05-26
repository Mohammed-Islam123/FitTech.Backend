using Membership.Domain.Enums;

namespace Membership.Features.Members.GetMyProfile;

/// <description>
/// Response containing the authenticated member's full profile.
/// </description>
public record GetMyProfileResponse(
    string FullName,
    string? Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string Email,
    bool EmailConfirmed,
    DateTime AccountCreationDate,
    int MembershipDurationYears,
    bool IsActive,
    string? ProfilePictureUrl,
    string? Goals,
    Guid? MedicalFileId
);
