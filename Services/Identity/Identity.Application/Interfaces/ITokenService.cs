using Identity.Domain.Entities;

namespace Identity.Application.Interfaces;

public interface ITokenService
{
    string GenerateUserToken(User user, IList<string> roles, string clientId);
    string GenerateServiceToken(string clientId, IList<string>? scopes = null);
}
