using Grpc.Core;
using Identity.Application.Interfaces;
using Shared.Protos;

namespace Identity.Api.Services;

public class IdentityProfileGrpcService : IdentityProfileService.IdentityProfileServiceBase
{
    private readonly IUserService _userService;

    public IdentityProfileGrpcService(IUserService userService)
    {
        _userService = userService;
    }

    public override async Task<ProfileBatchResponse> GetMemberProfiles(
        ProfileBatchRequest request, ServerCallContext context)
    {
        var userIds = request.UserIds
            .Select(id => Guid.TryParse(id, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        var profiles = await _userService.GetProfilesBatchAsync(userIds);

        var response = new ProfileBatchResponse();
        foreach (var p in profiles)
        {
            response.Profiles.Add(new ProfileMessage
            {
                UserId = p.UserId.ToString(),
                Email = p.Email ?? "",
                PhoneNumber = p.PhoneNumber ?? "",
                ProfilePhotoUrl = p.ProfilePhotoUrl ?? ""
            });
        }

        return response;
    }
}
