using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AppleAuthService
{
    private readonly HttpClient _httpClient;

    public AppleAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ExchangeCodeAsync(string code, string clientId, string clientSecret, string redirectUri = null)
    {
        var values = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "code", code },
            { "grant_type", "authorization_code" }
        };

        if (!string.IsNullOrEmpty(redirectUri))
        {
            values.Add("redirect_uri", redirectUri);
        }

        var content = new FormUrlEncodedContent(values);
        var response = await _httpClient.PostAsync("https://appleid.apple.com/auth/token", content);

        var respContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Apple token request failed: {respContent}");
        }

        return respContent; // JSON: access_token, id_token, refresh_token, expires_in
    }
}
