using Refit;
using Shared.Wrappers;

namespace Payment.Infrastructure;

public interface IIdentityServiceClient
{
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
    public string? ProfilePhotoUrl { get; set; }
}
