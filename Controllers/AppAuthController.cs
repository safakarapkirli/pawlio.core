using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AppleAuthController : ControllerBase
{
    public AppleAuthController()
    {
    }

    [HttpPost()]
    public IActionResult PawlioAppleCallback([FromForm] AppleAuthResponse data)
    {
        if (string.IsNullOrEmpty(data.Code))
            return Content("Authentication failed: Missing authorization code.", "text/html");

        if (string.IsNullOrEmpty(data.IdToken))
            return Content("Authentication failed: Missing id token.", "text/html");

        var id = GetAppleUserIdentifierFromToken(data.IdToken, out string email);
        var jsonData = "";

        if (!string.IsNullOrEmpty(data.User))
        {
            var userInfo = data.User!.JsonTo<AppleUserInfo>();
            var firstName = userInfo?.Name?.FirstName;
            var lastName = userInfo?.Name?.LastName;

            jsonData = JsonConvert.SerializeObject(new
            {
                id,
                firstName,
                lastName,
                email,
                token = data.IdToken
            });
        }
        else
        {
            jsonData = JsonConvert.SerializeObject(new
            {
                id,
                email,
                token = data.IdToken
            });
        }

        var androidIntentUrl = $"intent://callback?code={Uri.EscapeDataString(data.Code)}&id_token={Uri.EscapeDataString(jsonData)}#Intent;package=com.pawlio.app;scheme=signinwithapple;end";
        var htmlContent = $@"
        <!DOCTYPE html>
        <html>
            <head>
                <meta charset=""utf-8"">
                <title>Authentication</title>
            </head>
            <body>
                <script>
                    window.location.href = '{androidIntentUrl}';
                </script>
                Redirecting to your application...<br/>
            </body>
        </html>";

        // Oluşturulan HTML içeriğini tarayıcıya gönder
        return Content(htmlContent, "text/html");
    }

    //[HttpGet("burnx")]
    //public IActionResult AppleCallbackGet()
    //{
    //    return Ok("Its Work");
    //}

    //[HttpPost("burnx")]
    //public IActionResult AppleCallback([FromForm] AppleAuthResponse data)
    //{
    //    if (string.IsNullOrEmpty(data.Code))
    //        return Content("Authentication failed: Missing authorization code.", "text/html");

    //    if (string.IsNullOrEmpty(data.IdToken))
    //        return Content("Authentication failed: Missing id token.", "text/html");

    //    var id = GetAppleUserIdentifierFromToken(data.IdToken, out string email);
    //    var jsonData = "";

    //    if (!string.IsNullOrEmpty(data.User))
    //    {
    //        var userInfo = data.User!.JsonTo<AppleUserInfo>();
    //        var firstName = userInfo?.Name?.FirstName;
    //        var lastName = userInfo?.Name?.LastName;

    //        jsonData = JsonConvert.SerializeObject(new
    //        {
    //            id,
    //            firstName,
    //            lastName,
    //            email,
    //            token = data.IdToken
    //        });
    //    }
    //    else
    //    {
    //        jsonData = JsonConvert.SerializeObject(new
    //        {
    //            id,
    //            email,
    //            token = data.IdToken
    //        });
    //    }

    //    var androidIntentUrl = $"intent://callback?code={Uri.EscapeDataString(data.Code)}&id_token={Uri.EscapeDataString(jsonData)}#Intent;package=com.burnxlabs.burnx;scheme=signinwithapple;end";
    //    var htmlContent = $@"
    //    <!DOCTYPE html>
    //    <html>
    //        <head>
    //            <meta charset=""utf-8"">
    //            <title>Authentication</title>
    //        </head>
    //        <body>
    //            <script>
    //                window.location.href = '{androidIntentUrl}';
    //            </script>
    //            Redirecting to your application...<br/>
    //        </body>
    //    </html>";

    //    // Oluşturulan HTML içeriğini tarayıcıya gönder
    //    return Content(htmlContent, "text/html");
    //}

    [NonAction]
    public string? GetAppleUserIdentifierFromToken(string idToken, out string email)
    {
        email = string.Empty;

        if (string.IsNullOrEmpty(idToken))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();

            // Sadece token'ı okuyun, geçerliliğini kontrol etmeyin.
            // Bu, token'ın içindeki verilere hızlıca erişmek için kullanılır.
            var jsonToken = handler.ReadToken(idToken) as JwtSecurityToken;

            if (jsonToken == null)
            {
                // Geçersiz bir token formatı.
                return null;
            }

            var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email");
            if (emailClaim != null) email = emailClaim.Value;

            var appleIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
            if (appleIdClaim != null) return appleIdClaim.Value;

            var appleIdClaimAlternative = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (appleIdClaimAlternative != null) return appleIdClaimAlternative.Value;

            return null;
        }
        catch (Exception ex)
        {
            // Token'ı çözümleme sırasında bir hata oluştu.
            // Loglama yapılması tavsiye edilir.
            Console.WriteLine($"Token çözümleme hatası: {ex.Message}");
            return null;
        }
    }

    //public async Task<IActionResult> AppleLogin([FromForm] string code)
    //{
    //    var clientSecret = AppleJwtGenerator.CreateClientSecret(
    //        teamId: "3MSWY4CBDD", // TEAM_ID
    //        clientId: "com.burnxlabs.burnx",
    //        keyId: "V4G9SQB62R",
    //        privateKeyContent: System.IO.File.ReadAllText("AuthKey_V4G9SQB62R.p8")
    //    );

    //    // 2) code ile token al
    //    var tokenResponse = await _appleAuth.ExchangeCodeAsync(code, "com.burnxlabs.burnx", clientSecret);

    //    // 3) id_token doğrula
    //    var json = System.Text.Json.JsonDocument.Parse(tokenResponse);
    //    var idToken = json.RootElement.GetProperty("id_token").GetString();
    //    var jwt = await _tokenValidator.ValidateAsync(idToken, "com.burnxlabs.burnx");

    //    var appleUserId = jwt.Subject; // Apple'ın unique user id'si
    //    var email = jwt.Payload.ContainsKey("email") ? jwt.Payload["email"]?.ToString() : null;

    //    // 4) kullanıcı veritabanında yoksa oluştur
    //    // ...

    //    return Ok(new { appleUserId, email });
    //}

    public class AppleAuthResponse
    {
        [FromForm(Name = "code")]
        public string? Code { get; set; }

        [FromForm(Name = "id_token")]
        public string? IdToken { get; set; }

        [FromForm(Name = "user")]
        public string? User { get; set; }
    }

    public class AppleUserInfo
    {
        public AppleUserName Name { get; set; } = null!;
        public string Email { get; set; } = string.Empty;
    }

    public class AppleUserName
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
