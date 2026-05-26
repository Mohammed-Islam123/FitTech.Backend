using Microsoft.AspNetCore.Http;

namespace Membership.Features.Members.UpdateMyProfile;

public record UpdateMyProfileRequest(
    IFormFile? MedicalFile,
    string? Goals,
    IFormFile? ProfilePicture
);
