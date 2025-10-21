using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FSI.BusinessProcessManagement.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public string GenerateToken(IEnumerable<Claim> claims, DateTime? expires = null)
        {
            var (token, _) = GenerateTokenWithExpiry(claims, expires);
            return token;
        }

        public (string token, DateTime ExpiresAtUtc) GenerateTokenWithExpiry(IEnumerable<Claim> claims, DateTime? expires = null)
        {
            var jwt = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var exp = expires ?? DateTime.UtcNow.AddMinutes(int.TryParse(jwt["ExpiresMinutes"], out var m) ? m : 60);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: exp,
                signingCredentials: creds
            );

            var jwtStr = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwtStr, exp);
        }
    }
}
