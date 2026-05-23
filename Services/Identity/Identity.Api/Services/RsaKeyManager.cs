using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Services;

public sealed class RsaKeyManager
{
    public RsaSecurityKey PrivateKey { get; }
    public JsonWebKey PublicJwk { get; }

    public RsaKeyManager()
    {
        var rsa = RSA.Create(2048);
        PrivateKey = new RsaSecurityKey(rsa) { KeyId = Guid.NewGuid().ToString() };
        PublicJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(PrivateKey);
        PublicJwk.Alg = "RS256";
    }
}
