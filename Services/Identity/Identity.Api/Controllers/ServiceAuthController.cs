using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class ServiceAuthController(ITokenService tokenService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("service-token")]
    public IActionResult GetServiceToken([FromBody] ServiceTokenRequest request)
    {
        var clients = configuration.GetSection("ServiceClients")
            .Get<Dictionary<string, string>>();

        if (clients is null ||
            !clients.TryGetValue(request.ClientId, out var secret) ||
            secret != request.ClientSecret)
        {
            return Unauthorized(new { error = "Invalid client credentials" });
        }

        var token = tokenService.GenerateServiceToken(request.ClientId);
        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = 300
        });
    }
}

public record ServiceTokenRequest(string ClientId, string ClientSecret);
