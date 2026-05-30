using System.Security.Cryptography;
using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Infrastructure;
using ErrorOr;
using Shared.Events;
using Shared.Wrappers;
using Refit;
using Wolverine;

namespace Courses.Features.Coaches.CreateCoach;

/// <description>
/// Creates a new coach by registering the user in the Identity service, then creating the local coach profile.
/// </description>
public class CreateCoachHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IIdentityServiceClient identityClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<CreateCoachResponse>> Handle(
        CreateCoachCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Administrators can create coach profiles.");
        }

        var req = command.Request;
        var generatedPassword = GenerateSecurePassword(16);

        ApiResponse<Response<string>> identityResponse;
        try
        {
            identityResponse = await identityClient.CreateCoachUserAsync(
                userName: req.Email,
                email: req.Email,
                password: generatedPassword,
                firstName: req.FirstName,
                lastName: req.LastName,
                phoneNumber: req.PhoneNumber,
                dateOfBirth: req.DateOfBirth?.ToString("yyyy-MM-dd"),
                gender: req.Gender?.ToString(),
                isCoach: true);
        }
        catch (Exception ex)
        {
            return Error.Failure("Identity.ConnectionFailed",
                $"Failed to connect to Identity service: {ex.Message}");
        }
        if (!identityResponse.IsSuccessStatusCode || identityResponse.Content is null || !identityResponse.Content.Success)
        {
            return Error.Failure("Identity.CreateFailed",
                identityResponse.Error?.Content ?? "Failed to create coach account in Identity Service.");
        }

        if (!Guid.TryParse(identityResponse.Content.Data, out var userId))
        {
            return Error.Failure("Identity.InvalidUserId", "Identity service returned an invalid user ID.");
        }

        var coach = new Coach
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email,
            PhoneNumber = req.PhoneNumber,
            Bio = req.Bio
        };

        context.Coaches.Add(coach);
        await context.SaveChangesAsync(ct);

        var coachCreatedEvent = new CoachCreatedEvent(
            CoachId: coach.Id,
            UserId: userId,
            FirstName: req.FirstName,
            LastName: req.LastName,
            Email: req.Email);

        await messageBus.PublishAsync(coachCreatedEvent);

        return new CreateCoachResponse(coach.Id);
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
