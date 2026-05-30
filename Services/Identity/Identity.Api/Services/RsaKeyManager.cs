using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Services;

/// <description>
/// Manages the RSA key pair used for JWT signing and validation.
/// On first startup, generates a new 2048-bit RSA key and persists it to disk.
/// On subsequent startups, loads the existing key from disk so tokens survive restarts.
/// </description>
public sealed class RsaKeyManager
{
    private const string KeyFileName = "rsa-key.pem";

    public RsaSecurityKey PrivateKey { get; }
    public JsonWebKey PublicJwk { get; }

    public RsaKeyManager(IConfiguration configuration)
    {
        var keyDirectory = configuration["JwtSettings:KeyPath"] ?? "keys";
        Directory.CreateDirectory(keyDirectory);

        var keyFilePath = Path.Combine(keyDirectory, KeyFileName);

        RSA rsa;

        if (File.Exists(keyFilePath))
        {
            var pemBytes = File.ReadAllBytes(keyFilePath);
            rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(pemBytes, out _);
        }
        else
        {
            rsa = RSA.Create(2048);
            var pemBytes = rsa.ExportRSAPrivateKey();
            File.WriteAllBytes(keyFilePath, pemBytes);
        }

        PrivateKey = new RsaSecurityKey(rsa) { KeyId = ComputeKeyId(rsa) };
        PublicJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(PrivateKey);
        PublicJwk.Alg = "RS256";
    }

    private static string ComputeKeyId(RSA rsa)
    {
        var publicKeyBytes = rsa.ExportRSAPublicKey();
        var hash = SHA256.HashData(publicKeyBytes);
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
