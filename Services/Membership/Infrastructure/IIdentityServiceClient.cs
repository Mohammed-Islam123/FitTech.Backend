using Membership.Domain.Enums;
using Refit;
using Shared.Wrappers;

namespace Membership.Infrastructure;

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
}
