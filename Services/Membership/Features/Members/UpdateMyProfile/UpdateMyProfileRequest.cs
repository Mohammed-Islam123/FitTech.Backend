using Membership.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Membership.Features.Members.UpdateMyProfile;

/// <description>
/// All fields are optional — send only what you want to change (PATCH semantics).
/// </description>
public record UpdateMyProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    Gender? Gender,
    DateOnly? DateOfBirth,
    IFormFile? MedicalFile,
    string? Goals,
    string? MedicalRestrictions,
    IFormFile? ProfilePicture,
    string? OldPassword,
    string? NewPassword
);
