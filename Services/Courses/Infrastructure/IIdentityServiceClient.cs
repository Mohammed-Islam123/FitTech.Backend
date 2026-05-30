using Refit;
using Shared.Wrappers;

namespace Courses.Infrastructure;

public interface IIdentityServiceClient
{
    [Multipart]
    [Post("/api/User/register")]
    Task<ApiResponse<Response<string>>> CreateCoachUserAsync(
        [AliasAs("UserName")] string userName,
        [AliasAs("Email")] string email,
        [AliasAs("Password")] string password,
        [AliasAs("FirstName")] string firstName,
        [AliasAs("LastName")] string lastName,
        [AliasAs("PhoneNumber")] string phoneNumber,
        [AliasAs("DateOfBirth")] string? dateOfBirth,
        [AliasAs("Gender")] string? gender,
        [AliasAs("IsCoach")] bool isCoach);

    [Get("/api/User/profile/{userId}")]
    Task<ApiResponse<Response<IdentityProfileDto>>> GetProfileAsync(Guid userId);

    [Get("/api/User/{userId}/medical-file")]
    Task<ApiResponse<Response<IdentityMedicalFileDto>>> GetMedicalFileAsync(Guid userId);
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
