using Membership.Domain.Enums;
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
        [AliasAs("DateOfBirth")] DateOnly dateOfBirth,
        [AliasAs("Gender")] Gender gender,
        [AliasAs("MedicalFile")] StreamPart? medicalFile,
        [AliasAs("ProfilePicture")] StreamPart? profilePicture);

    [Multipart]
    [Put("/api/User/profile")]
    Task<ApiResponse<Response<string>>> UpdateProfileAsync(
        [AliasAs("UserId")] Guid userId,
        [AliasAs("FirstName")] string firstName,
        [AliasAs("LastName")] string lastName,
        [AliasAs("PhoneNumber")] string phoneNumber,
        [AliasAs("Gender")] Gender? gender,
        [AliasAs("DateOfBirth")] DateOnly? dateOfBirth,
        [AliasAs("ProfilePicture")] StreamPart? profilePicture);

    [Post("/api/User/change-password")]
    Task<ApiResponse<Response<string>>> ChangePasswordAsync([Body] ChangePasswordRequest dto);

    [Put("/api/User/{userId}/deactivate")]
    Task<ApiResponse<Response<string>>> DeactivateUserAsync(Guid userId);

    [Get("/api/User/profile/{userId}")]
    Task<ApiResponse<Response<IdentityProfileDto>>> GetProfileAsync(Guid userId);

    [Get("/api/User/{userId}/medical-file")]
    Task<ApiResponse<Response<IdentityMedicalFileDto>>> GetMedicalFileAsync(Guid userId);

    [Multipart]
    [Post("/api/User/medical-file")]
    Task<ApiResponse<Response<IdentityMedicalFileDto>>> UploadMedicalFileAsync(
        [AliasAs("UserId")] Guid userId,
        [AliasAs("File")] StreamPart file);
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
}

public class IdentityMedicalFileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}
