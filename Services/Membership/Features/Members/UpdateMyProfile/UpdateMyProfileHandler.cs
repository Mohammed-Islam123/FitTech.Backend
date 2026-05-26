using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace Membership.Features.Members.UpdateMyProfile;

/// <description>
/// Updates the authenticated member's profile: medical file, goals, and/or profile picture.
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

        await context.SaveChangesAsync(ct);

        return new UpdateMyProfileResponse(member.Id);
    }
}
