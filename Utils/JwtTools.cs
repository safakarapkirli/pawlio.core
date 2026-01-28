using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace bagtofly.net6.Utils
{
    public static class JwtTools
    {
        private static IConfigurationRoot? _configuration;

        public static JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            if (_configuration == null) _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
