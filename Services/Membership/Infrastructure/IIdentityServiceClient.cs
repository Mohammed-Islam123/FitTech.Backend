using Refit;
using Shared.Wrappers;

namespace Membership.Infrastructure;

public record CreateUserResponse(bool Success, string Data, CreateUserError? Error);
public record CreateUserError(string Content);

public interface IIdentityServiceClient
{
    [Multipart]
    [Post("/api/User/register")]
    Task<ApiResponse<Response<string>>> CreateUserAsync(
        [AliasAs("UserName")] string userName,
        [AliasAs("Email")] string email,
        [AliasAs("Password")] string password,
        [AliasAs("FirstName")] string firstName,
        [AliasAs("LastName")] string lastName,
        [AliasAs("PhoneNumber")] string phoneNumber,
        [AliasAs("DateOfBirth")] string dateOfBirth,
        [AliasAs("Gender")] string gender,
        [AliasAs("MedicalFile")] StreamPart? medicalFile,
        [AliasAs("ProfilePicture")] StreamPart? profilePicture);

    [Multipart]
    [Put("/api/User/profile")]
    Task<ApiResponse<Response<string>>> UpdateProfileAsync(
        [AliasAs("UserId")] Guid userId,
        [AliasAs("FirstName")] string firstName,
        [AliasAs("LastName")] string lastName,
        [AliasAs("PhoneNumber")] string phoneNumber,
        [AliasAs("Gender")] string? gender,
        [AliasAs("DateOfBirth")] string? dateOfBirth,
        [AliasAs("ProfilePicture")] StreamPart? profilePicture);

    [Post("/api/User/change-password")]
    Task<ApiResponse<Response<string>>> ChangePasswordAsync([Body] ChangePasswordRequest dto);

    [Put("/api/User/{userId}/deactivate")]
    Task<ApiResponse<Response<string>>> DeactivateUserAsync(Guid userId);

    [Get("/api/User/profile/{userId}")]
    Task<ApiResponse<Response<IdentityProfileDto>>> GetProfileAsync(Guid userId);

}

public class IdentityProfileDto
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool EmailConfirmed { get; set; }
}

