using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

public static class AppleJwtGenerator
{
    public static string CreateClientSecret(string teamId, string clientId, string keyId, string privateKeyContent)
    {
        var handler = new JwtSecurityTokenHandler();

        // p8 dosyasındaki başlık/son satırları temizle
        privateKeyContent = privateKeyContent
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");

        // Base64 decode
        var keyData = Convert.FromBase64String(privateKeyContent);

        // Apple ES256 = ECDSA P-256 curve
        var ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(keyData, out _);

        var securityKey = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = keyId
        };

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

        var now = DateTimeOffset.UtcNow;
        var token = new JwtSecurityToken(
            issuer: teamId,
            audience: "https://appleid.apple.com",
            claims: null,
            notBefore: now.UtcDateTime,
            expires: now.AddDays(180).UtcDateTime, // max 6 ay
            signingCredentials: credentials
        );

        token.Payload["sub"] = clientId;

        return handler.WriteToken(token);
    }
}