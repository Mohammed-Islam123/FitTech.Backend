using Membership.Domain.Enums;
using Refit;

namespace Membership.Infrastructure;

public interface IIdentityServiceClient
{
    [Post("/identity/users")]
    Task<ApiResponse<CreateUserResponse>> CreateUserAsync(CreateUserRequest request);
}

public record CreateUserRequest(
    string Email,
    string Password,
    string Role,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    Gender Gender);

public record CreateUserResponse(Guid UserId);