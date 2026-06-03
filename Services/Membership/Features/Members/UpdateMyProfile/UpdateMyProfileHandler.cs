using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace Membership.Features.Members.UpdateMyProfile;

/// <description>
/// Updates the authenticated member's profile: medical file, goals, and/or profile picture.
/// Also supports password change via old/new password fields.
/// Medical files are stored locally in the Membership service.
/// </description>
public class UpdateMyProfileHandler(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor,
    IWebHostEnvironment environment)
{
    public async Task<ErrorOr<UpdateMyProfileResponse>> Handle(
        UpdateMyProfileCommand command,
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
        {
            return Error.Unauthorized(
                "Profile.Unauthorized",
                "Authentication required to update profile.");
        }

        var member = await context.Members
            .Include(m => m.HealthProfile)
            .FirstOrDefaultAsync(m => m.UserId == currentUserId, ct);

        if (member is null)
        {
            return Error.NotFound(
                "Member.NotFound",
                "No member record found for the current user.");
        }

        var req = command.Request;

        // --- 1. Upload Medical File locally ---
        if (req.MedicalFile is not null)
        {
            var fileId = Guid.CreateVersion7();
            var ext = Path.GetExtension(req.MedicalFile.FileName) ?? ".bin";
            var fileName = $"{fileId}{ext}";
            var memberDir = Path.Combine(
                environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
                "medical-files",
                member.Id.ToString());
            Directory.CreateDirectory(memberDir);
            var filePath = Path.Combine(memberDir, fileName);

            await using var stream = req.MedicalFile.OpenReadStream();
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, ct);

            var fileUrl = $"/medical-files/{member.Id}/{fileName}";

            if (member.HealthProfile is null)
            {
                member.HealthProfile = new Domain.Entities.MemberHealthProfile
                {
                    MemberId = member.Id,
                    MedicalFileUrl = fileUrl,
                    MedicalFileName = req.MedicalFile.FileName,
                    LastUpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                member.HealthProfile.MedicalFileUrl = fileUrl;
                member.HealthProfile.MedicalFileName = req.MedicalFile.FileName;
                member.HealthProfile.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        // --- 2. Upload Profile Picture to Identity ---
        if (req.ProfilePicture is not null)
        {
            var profileStreamPart = new StreamPart(
                req.ProfilePicture.OpenReadStream(),
                req.ProfilePicture.FileName,
                req.ProfilePicture.ContentType);

            try
            {
                var profileResponse = await identityClient.UpdateProfileAsync(
                    userId: currentUserId.Value,
                    firstName: member.FirstName,
                    lastName: member.LastName,
                    phoneNumber: null!,
                    gender: null,
                    dateOfBirth: null,
                    profilePicture: profileStreamPart);

                if (!profileResponse.IsSuccessStatusCode || profileResponse.Content?.Data is null)
                {
                    return Error.Failure(
                        "ProfilePicture.UploadFailed",
                        "Failed to upload profile picture to Identity service.");
                }
            }
            finally
            {
                profileStreamPart.Value?.Dispose();
            }
        }

        // --- 3. Update Goals ---
        if (req.Goals is not null)
        {
            if (member.HealthProfile is null)
            {
                member.HealthProfile = new Domain.Entities.MemberHealthProfile
                {
                    MemberId = member.Id,
                    Objectives = req.Goals,
                    LastUpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                member.HealthProfile.Objectives = req.Goals;
                member.HealthProfile.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        // --- 4. Change Password ---
        if (!string.IsNullOrWhiteSpace(req.OldPassword) && !string.IsNullOrWhiteSpace(req.NewPassword))
        {
            var passwordResponse = await identityClient.ChangePasswordAsync(
                new ChangePasswordRequest(req.OldPassword, req.NewPassword));

            if (!passwordResponse.IsSuccessStatusCode || passwordResponse.Content is null || !passwordResponse.Content.Success)
            {
                return Error.Validation(
                    "Identity.PasswordChangeFailed",
                    passwordResponse.Content?.Message ?? "Failed to change password. Ensure the old password is correct.");
            }
        }

        await context.SaveChangesAsync(ct);

        return new UpdateMyProfileResponse(member.Id);
    }
}
