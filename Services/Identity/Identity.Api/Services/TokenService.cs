using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Services;

public sealed class TokenService(RsaKeyManager rsaKeyManager, IConfiguration configuration) : ITokenService
{
    public string GenerateUserToken(User user, IList<string> roles, string clientId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
            new("client_id", clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("type", "user")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(rsaKeyManager.PrivateKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(configuration["JwtSettings:AccessTokenExpirationMinutes"]!)),
            claims: claims,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateServiceToken(string clientId, IList<string>? scopes = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("type", "service"),
            new("client_id", clientId)
        };

        if (scopes is not null)
            claims.AddRange(scopes.Select(s => new Claim("scope", s)));

        var creds = new SigningCredentials(rsaKeyManager.PrivateKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            expires: DateTime.UtcNow.AddMinutes(5),
            claims: claims,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
