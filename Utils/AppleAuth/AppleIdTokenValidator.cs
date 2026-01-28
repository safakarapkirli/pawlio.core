using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;

public class AppleIdTokenValidator
{
    private readonly HttpClient _httpClient;

    public AppleIdTokenValidator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JwtSecurityToken> ValidateAsync(string idToken, string clientId)
    {
        var keysResponse = await _httpClient.GetFromJsonAsync<AppleKeys>("https://appleid.apple.com/auth/keys");

        var handler = new JwtSecurityTokenHandler();
        foreach (var key in keysResponse.Keys)
        {
            var e = Base64UrlEncoder.DecodeBytes(key.E);
            var n = Base64UrlEncoder.DecodeBytes(key.N);

            var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Exponent = e,
                Modulus = n
            });

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsa)
            };

            try
            {
                handler.ValidateToken(idToken, validationParameters, out var validatedToken);
                return (JwtSecurityToken)validatedToken;
            }
            catch
            {
                // diğer key ile dene
            }
        }

        throw new SecurityTokenException("Apple ID token doğrulama başarısız");
    }
}

public class AppleKeys
{
    public List<AppleJwk> Keys { get; set; }
}

public class AppleJwk
{
    public string Kty { get; set; }
    public string Kid { get; set; }
    public string Use { get; set; }
    public string Alg { get; set; }
    public string N { get; set; }
    public string E { get; set; }
}
