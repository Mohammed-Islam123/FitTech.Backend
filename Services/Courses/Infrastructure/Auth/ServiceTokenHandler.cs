using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Courses.Infrastructure.Auth;

public sealed class ServiceTokenHandler : DelegatingHandler
{
    private readonly HttpClient _identityClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<ServiceTokenHandler> _logger;

    private string? _cachedToken;
    private DateTime _tokenExpiry;

    public ServiceTokenHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ServiceTokenHandler> logger)
    {
        _identityClient = httpClientFactory.CreateClient("IdentityAuth");
        _clientId = configuration["ServiceAuth:ClientId"]!;
        _clientSecret = configuration["ServiceAuth:ClientSecret"]!;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            _logger.LogDebug("Fetching new service token for {ClientId}", _clientId);

            var response = await _identityClient.PostAsJsonAsync(
                "/auth/service-token",
                new { clientId = _clientId, clientSecret = _clientSecret },
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>(ct)
                ?? throw new InvalidOperationException("Null service token response");

            _cachedToken = result.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 30);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}

public sealed record ServiceTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType);
