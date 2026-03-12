using Identity.Domain.Entities;

namespace Identity.Application.Interfaces ;

public interface ITokenService
{
    string GenerateAccessToken(User user, IList<string> roles, string clientId);
}
