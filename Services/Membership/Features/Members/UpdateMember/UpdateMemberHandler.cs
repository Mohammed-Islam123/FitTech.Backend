using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Membership.Infrastructure;
using Refit;
using Wolverine;

namespace Membership.Features.Members.UpdateMember;

public class UpdateMemberHandler(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<UpdateMemberResponse>> Handle(UpdateMemberCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var currentUserId = userAccessor.UserId;
        var isAdmin = userAccessor.IsAdmin;

        Member? member;

        if (command.Id.HasValue)
        {
            // Admin updating a specific member
            if (!isAdmin)
            {
                return Error.Unauthorized("Member.Unauthorized", "Only Admins can update other members.");
            }

            member = await context.Members
                .Include(m => m.HealthProfile)
                .FirstOrDefaultAsync(m => m.Id == command.Id.Value, ct);
        }
        else
        {
            // Self-update
            member = await context.Members
                .Include(m => m.HealthProfile)
                .FirstOrDefaultAsync(m => m.UserId == currentUserId, ct);
        }

        if (member is null)
        {
            return Error.NotFound("Member.NotFound", "The specified member was not found.");
        }

        // 1. Update Identity Service Profile
        StreamPart? profileStreamPart = null;
        try
        {
            if (req.ProfilePicture != null)
            {
                profileStreamPart = new StreamPart(
                    req.ProfilePicture.OpenReadStream(),
                    req.ProfilePicture.FileName,
                    req.ProfilePicture.ContentType);
            }

            var identityResponse = await identityClient.UpdateProfileAsync(
                userId: member.UserId,
                firstName: req.FirstName,
                lastName: req.LastName,
                phoneNumber: req.PhoneNumber,
                gender: req.Gender,
                dateOfBirth: req.DateOfBirth,
                profilePicture: profileStreamPart
            );

            if (!identityResponse.IsSuccessStatusCode || identityResponse.Content is null || !identityResponse.Content.Success)
            {
                return Error.Failure("Identity.UpdateFailed",
                    identityResponse.Error?.Content ?? "Failed to update profile in Identity Service.");
            }

            // 2. Change Password if requested (only for self-update or if business rules allow)
            if (!string.IsNullOrWhiteSpace(req.OldPassword) && !string.IsNullOrWhiteSpace(req.NewPassword))
            {
                // Only allow self-password change via this endpoint
                if (member.UserId != currentUserId)
                {
                    return Error.Unauthorized("Member.PasswordUnauthorized", "You can only change your own password.");
                }

                var passwordResponse = await identityClient.ChangePasswordAsync(new ChangePasswordRequest(
                    req.OldPassword,
                    req.NewPassword
                ));

                if (!passwordResponse.IsSuccessStatusCode || passwordResponse.Content is null || !passwordResponse.Content.Success)
                {
                    return Error.Validation("Identity.PasswordChangeFailed",
                        passwordResponse.Content?.Message ?? "Failed to change password. Ensure the old password is correct.");
                }
            }
        }
        finally
        {
            profileStreamPart?.Value?.Dispose();
        }

        // 3. Update Membership Member Entity
        member.FirstName = req.FirstName;
        member.LastName = req.LastName;

        if (isAdmin && req.Status.HasValue)
        {
            member.Status = req.Status.Value;
        }

        // 4. Update Health Profile - ONLY if self-update
        if (member.UserId == currentUserId)
        {
            if (member.HealthProfile == null)
            {
                member.HealthProfile = new MemberHealthProfile
                {
                    MemberId = member.Id,
                    Objectives = req.Objectives,
                    MedicalRestrictions = req.MedicalRestrictions,
                    LastUpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                member.HealthProfile.Objectives = req.Objectives;
                member.HealthProfile.MedicalRestrictions = req.MedicalRestrictions;
                member.HealthProfile.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);

        return new UpdateMemberResponse(member.Id);
    }
}
