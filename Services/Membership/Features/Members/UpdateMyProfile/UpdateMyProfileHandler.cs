using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace Membership.Features.Members.UpdateMyProfile;

/// <description>
/// Updates the authenticated member's profile via PATCH semantics.
/// Send only the fields you want to change — all fields are optional.
/// Identity fields (name, phone, gender, DoB, profile picture) are forwarded to the Identity service.
/// Password change is handled via the Identity service.
/// </description>
public class UpdateMyProfileHandler(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor)
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

        // --- 1. Update Identity Service Profile (if any identity field changed) ---
        var needsIdentityUpdate =
            req.FirstName is not null
            || req.LastName is not null
            || req.PhoneNumber is not null
            || req.Gender.HasValue
            || req.DateOfBirth.HasValue
            || req.ProfilePicture is not null;

        if (needsIdentityUpdate)
        {
            // Fetch current profile from Identity to fill required fields we aren't changing
            var currentProfile = await GetCurrentIdentityProfile(currentUserId.Value);

            var firstName = req.FirstName ?? currentProfile.FirstName ?? member.FirstName;
            var lastName = req.LastName ?? currentProfile.LastName ?? member.LastName;
            var phoneNumber = req.PhoneNumber ?? currentProfile.PhoneNumber ?? string.Empty;
            var gender = req.Gender?.ToString() ?? currentProfile.Gender;
            var dateOfBirth = req.DateOfBirth?.ToString("yyyy-MM-dd") ?? currentProfile.DateOfBirth;

            StreamPart? profileStreamPart = null;
            try
            {
                if (req.ProfilePicture is not null)
                {
                    profileStreamPart = new StreamPart(
                        req.ProfilePicture.OpenReadStream(),
                        req.ProfilePicture.FileName,
                        req.ProfilePicture.ContentType);
                }

                var identityResponse = await identityClient.UpdateProfileAsync(
                    userId: currentUserId.Value,
                    firstName: firstName,
                    lastName: lastName,
                    phoneNumber: phoneNumber,
                    gender: gender,
                    dateOfBirth: dateOfBirth,
                    profilePicture: profileStreamPart);

                if (!identityResponse.IsSuccessStatusCode || identityResponse.Content is null || !identityResponse.Content.Success)
                {
                    return Error.Failure(
                        "Identity.UpdateFailed",
                        identityResponse.Error?.Content ?? "Failed to update profile in Identity Service.");
                }
            }
            finally
            {
                profileStreamPart?.Value?.Dispose();
            }
        }

        // --- 2. Change Password (if requested) ---
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

        // --- 3. Upload Medical File (if provided) ---
        if (req.MedicalFile is not null)
        {
            var fileStreamPart = new StreamPart(
                req.MedicalFile.OpenReadStream(),
                req.MedicalFile.FileName,
                req.MedicalFile.ContentType);

            try
            {
                var uploadResponse = await identityClient.UploadMedicalFileAsync(
                    userId: currentUserId.Value,
                    file: fileStreamPart);

                if (!uploadResponse.IsSuccessStatusCode || uploadResponse.Content?.Data is null)
                {
                    return Error.Failure(
                        "MedicalFile.UploadFailed",
                        "Failed to upload medical file to Identity service.");
                }
            }
            finally
            {
                fileStreamPart.Value?.Dispose();
            }
        }

        // --- 4. Update local Member entity ---
        if (req.FirstName is not null)
        {
            member.FirstName = req.FirstName;
        }

        if (req.LastName is not null)
        {
            member.LastName = req.LastName;
        }

        // --- 5. Update Health Profile ---
        if (req.Goals is not null || req.MedicalRestrictions is not null)
        {
            if (member.HealthProfile is null)
            {
                member.HealthProfile = new Domain.Entities.MemberHealthProfile
                {
                    MemberId = member.Id,
                    Objectives = req.Goals,
                    MedicalRestrictions = req.MedicalRestrictions,
                    LastUpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                if (req.Goals is not null)
                {
                    member.HealthProfile.Objectives = req.Goals;
                }

                if (req.MedicalRestrictions is not null)
                {
                    member.HealthProfile.MedicalRestrictions = req.MedicalRestrictions;
                }

                member.HealthProfile.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);

        return new UpdateMyProfileResponse(member.Id);
    }

    /// <summary>
    /// Fetches the current Identity profile to use as fallback values for required fields
    /// that the caller did not include in a partial update.
    /// </summary>
    private async Task<CurrentIdentityProfile> GetCurrentIdentityProfile(Guid userId)
    {
        try
        {
            var response = await identityClient.GetProfileAsync(userId);
            if (response.IsSuccessStatusCode && response.Content?.Data is not null)
            {
                var p = response.Content.Data;
                return new CurrentIdentityProfile(
                    FirstName: p.FirstName,
                    LastName: p.LastName,
                    PhoneNumber: p.PhoneNumber,
                    Gender: p.Gender,
                    DateOfBirth: p.DateOfBirth?.ToString("yyyy-MM-dd"));
            }
        }
        catch
        {
            // Swallow — fall back to Member entity values for name, empty for phone
        }

        return new CurrentIdentityProfile(null, null, null, null, null);
    }

    private sealed record CurrentIdentityProfile(
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        string? Gender,
        string? DateOfBirth);
}
