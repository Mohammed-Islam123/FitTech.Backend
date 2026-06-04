using System.Text.Json;
using StackExchange.Redis;
using Shared.Protos;

namespace Membership.Infrastructure;

public class IdentityProfileCacheService
{
    private readonly IDatabase _cache;
    private readonly IdentityProfileService.IdentityProfileServiceClient _grpcClient;
    private readonly ILogger<IdentityProfileCacheService> _logger;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "identity:profile:";

    public IdentityProfileCacheService(
        IConnectionMultiplexer redis,
        IdentityProfileService.IdentityProfileServiceClient grpcClient,
        ILogger<IdentityProfileCacheService> logger)
    {
        _cache = redis.GetDatabase();
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<Dictionary<Guid, ProfileMessage>> GetProfilesAsync(List<Guid> userIds)
    {
        var result = new Dictionary<Guid, ProfileMessage>();
        var missed = new List<Guid>();

        foreach (var userId in userIds)
        {
            var cached = await _cache.StringGetAsync($"{CacheKeyPrefix}{userId}");
            if (cached.HasValue)
            {
                var profile = JsonSerializer.Deserialize<ProfileMessage>((string)cached!);
                if (profile is not null)
                {
                    result[userId] = profile;
                    continue;
                }
            }
            missed.Add(userId);
        }

        if (missed.Count > 0)
        {
            _logger.LogDebug("Cache miss for {Count} profiles, calling gRPC batch", missed.Count);

            var request = new ProfileBatchRequest();
            request.UserIds.AddRange(missed.Select(id => id.ToString()));

            var grpcResponse = await _grpcClient.GetMemberProfilesAsync(request);

            foreach (var profile in grpcResponse.Profiles)
            {
                if (Guid.TryParse(profile.UserId, out var uid))
                {
                    result[uid] = profile;
                    var json = JsonSerializer.Serialize(profile);
                    await _cache.StringSetAsync($"{CacheKeyPrefix}{uid}", json, CacheTtl);
                }
            }
        }
        else
        {
            _logger.LogDebug("All {Count} profiles served from cache", userIds.Count);
        }

        return result;
    }
}
