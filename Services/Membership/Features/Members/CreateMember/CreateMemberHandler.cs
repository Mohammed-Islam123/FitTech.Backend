using System.Security.Cryptography;
using ErrorOr;
using Membership.Domain;
using Membership.Domain.Entities;
using Membership.Domain.Enums;
using Membership.Infrastructure;
using Shared.Events;
using Refit;
using Wolverine;

namespace Membership.Features.Members.CreateMember;

public class CreateMemberHandler(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<CreateMemberResponse>> Handle(
        CreateMemberCommand command,
        CancellationToken ct)
    {
        var req = command.Request;

        var plan = await context.SubscriptionPlans.FindAsync([req.PlanId], cancellationToken: ct);
        if (plan is null || !plan.IsActive)
        {
            return Error.NotFound("SubscriptionPlan.NotFound", "The specified subscription plan does not exist or is inactive.");
        }

        if (!string.IsNullOrEmpty(req.CardUid))
        {
            var cardExists = context.NfcCards.Any(c => c.CardUid == req.CardUid);
            if (cardExists)
            {
                return Error.Conflict("NfcCard.AlreadyExists", "The provided NFC card is already assigned to another member.");
            }
        }

        var generatedPassword = GenerateSecurePassword(16);

        // Prepare file streams for Refit
        StreamPart? medicalStreamPart = null;
        StreamPart? profileStreamPart = null;

        try
        {
            if (req.MedicalCertificate != null)
            {
                medicalStreamPart = new StreamPart(
                    req.MedicalCertificate.OpenReadStream(),
                    req.MedicalCertificate.FileName,
                    req.MedicalCertificate.ContentType);
            }

            if (req.ProfilePicture != null)
            {
                profileStreamPart = new StreamPart(
                    req.ProfilePicture.OpenReadStream(),
                    req.ProfilePicture.FileName,
                    req.ProfilePicture.ContentType);
            }

            var identityResponse = await identityClient.CreateUserAsync(
                userName: req.Email, // Using email as username
                email: req.Email,
                password: generatedPassword,
                firstName: req.FirstName,
                lastName: req.LastName,
                phoneNumber: req.PhoneNumber,
                dateOfBirth: req.DateOfBirth,
                gender: req.Gender,
                medicalFile: medicalStreamPart,
                profilePicture: profileStreamPart
            );

            if (!identityResponse.IsSuccessStatusCode || identityResponse.Content is null || !identityResponse.Content.Success)
            {
                return Error.Failure("Identity.CreateFailed",
                    identityResponse.Error?.Content ?? "Failed to create user account in Identity Service.");
            }

            if (!Guid.TryParse(identityResponse.Content.Data, out var userId))
            {
                return Error.Failure("Identity.InvalidUserId", "Identity service returned an invalid user ID.");
            }

            var member = new Member
            {
                UserId = userId,
                FirstName = req.FirstName,
                LastName = req.LastName,
                JoinDate = DateTime.UtcNow,
                Status = MemberStatus.Active,
                NoShowWarningCount = 0
            };

            var startOnUtc = DateTime.UtcNow;
            DateTime? endOnUtc = null;

            if (plan.DurationUnit == DurationUnit.Months && plan.DurationValue.HasValue)
            {
                endOnUtc = startOnUtc.AddMonths(plan.DurationValue.Value);
            }
            else if (plan.DurationUnit == DurationUnit.Days && plan.DurationValue.HasValue)
            {
                endOnUtc = startOnUtc.AddDays(plan.DurationValue.Value);
            }

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                PlanId = plan.Id,
                StartOnUTC = startOnUtc,
                EndOnUTC = endOnUtc,
                RemainingSessions = plan.SessionCount,
                Status = SubscriptionStatus.Active
            };

            context.Members.Add(member);
            context.Subscriptions.Add(subscription);

            if (!string.IsNullOrEmpty(req.CardUid))
            {
                var nfcCard = new NfcCard
                {
                    Id = Guid.NewGuid(),
                    MemberId = member.Id,
                    CardUid = req.CardUid,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                };
                context.NfcCards.Add(nfcCard);
            }

            await context.SaveChangesAsync(ct);

            var memberCreatedEvent = new MemberCreatedEvent(
                MemberId: member.Id,
                UserId: userId,
                FullName: $"{req.FirstName} {req.LastName}",
                Email: req.Email,
                GeneratedPassword: generatedPassword,
                AssignedPlanName: plan.Name
            );

            await messageBus.PublishAsync(memberCreatedEvent);

            return new CreateMemberResponse(member.Id);
        }
        finally
        {
            // Ensure streams are disposed
            medicalStreamPart?.Value?.Dispose();
            profileStreamPart?.Value?.Dispose();
        }
    }

    private static string GenerateSecurePassword(int length)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()+-=[]{}|;:,.<>?";

        var charSet = upper + lower + digits + special;
        var result = new char[length];

        result[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        result[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        result[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        result[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (int i = 4; i < length; i++)
        {
            result[i] = charSet[RandomNumberGenerator.GetInt32(charSet.Length)];
        }

        RandomNumberGenerator.Shuffle(result);
        return new string(result);
    }
}


