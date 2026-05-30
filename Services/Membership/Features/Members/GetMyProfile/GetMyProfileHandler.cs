using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.GetMyProfile;

/// <description>
/// Retrieves the authenticated member's personal details by combining data from
/// the Membership database and the Identity service.
/// </description>
public class GetMyProfileHandler(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetMyProfileResponse>> Handle(
        GetMyProfileQuery query,
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
        {
            return Error.Unauthorized(
                "Profile.Unauthorized",
                "Authentication required to access profile.");
        }

        var member = await context.Members
            .AsNoTracking()
            .Include(m => m.HealthProfile)
            .FirstOrDefaultAsync(m => m.UserId == currentUserId, ct);

        if (member is null)
        {
            return Error.NotFound(
                "Member.NotFound",
                "No member record found for the current user.");
        }

        var identityProfileResponse = await identityClient.GetProfileAsync(currentUserId.Value);
        var identityProfile = identityProfileResponse.IsSuccessStatusCode
            ? identityProfileResponse.Content?.Data
            : null;

        var medicalFileResponse = await identityClient.GetMedicalFileAsync(currentUserId.Value);
        var medicalFile = medicalFileResponse.IsSuccessStatusCode
            ? medicalFileResponse.Content?.Data
            : null;

        var membershipDurationYears = (int)Math.Floor(
            (DateTime.UtcNow - member.JoinDate).TotalDays / 365.25);

        return new GetMyProfileResponse(
            FirstName: member.FirstName,
            LastName: member.LastName,
            Gender: identityProfile?.Gender,
            DateOfBirth: identityProfile?.DateOfBirth,
            PhoneNumber: identityProfile?.PhoneNumber,
            Email: identityProfile?.Email ?? string.Empty,
            EmailConfirmed: identityProfile?.EmailConfirmed ?? false,
            AccountCreationDate: member.JoinDate,
            MembershipDurationYears: membershipDurationYears,
            IsActive: member.Status == MemberStatus.Active,
            ProfilePictureUrl: identityProfile?.ProfilePhotoUrl,
            Goals: member.HealthProfile?.Objectives,
            MedicalFileId: medicalFile?.Id
        );
    }
}
