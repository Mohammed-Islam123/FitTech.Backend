using Identity.Api.Services;

namespace Identity.Api.Endpoints;

public static class WellKnownEndpoints
{
    public static void MapWellKnownEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/.well-known/openid-configuration", (HttpRequest request, IConfiguration config) =>
        {
            var issuer = config["JwtSettings:Issuer"]!;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return Results.Json(new
            {
                issuer,
                jwks_uri = $"{baseUrl}/.well-known/jwks",
                token_endpoint = $"{baseUrl}/auth/service-token",
                token_endpoint_auth_methods_supported = new[] { "client_secret_post" },
                id_token_signing_alg_values_supported = new[] { "RS256" }
            });
        });

        app.MapGet("/.well-known/jwks", (RsaKeyManager keyManager) =>
        {
            return Results.Json(new { keys = new[] { keyManager.PublicJwk } });
        });
    }
}
