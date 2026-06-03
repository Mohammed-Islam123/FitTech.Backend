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

