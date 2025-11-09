using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FSI.BusinessProcessManagement.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Services
{
    public sealed class TokenServiceTests
    {
        private static IConfiguration BuildConfig(
            string key = "test-secret-key-1234567890-abcdefghijklmnopqrstuvwxyz",
            string issuer = "bpm-api",
            string audience = "bpm-clients",
            string? expiresMinutes = "30") // pode passar null para simular ausência
        {
            var dict = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = issuer,
                ["Jwt:Audience"] = audience
            };
            if (expiresMinutes != null)
                dict["Jwt:ExpiresMinutes"] = expiresMinutes;

            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
        }

        private static (ClaimsPrincipal principal, JwtSecurityToken jwt) Validate(
            string token, string key, string issuer, string audience, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromSeconds(5)
            };

            var principal = handler.ValidateToken(token, parameters, out var validated);
            var jwt = Assert.IsType<JwtSecurityToken>(validated);
            Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
            return (principal, jwt);
        }

        private static IEnumerable<Claim> SampleClaims() => new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Name, "rodrigo"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        [Fact]
        public void GenerateTokenWithExpiry_ShouldEmbedClaims_Issuer_Audience_AndExplicitExpiry()
        {
            var cfg = BuildConfig(expiresMinutes: "15");
            var service = new TokenService(cfg);

            var claims = SampleClaims();
            var explicitExp = new DateTime(2030, 01, 01, 00, 00, 00, DateTimeKind.Utc);

            var (token, expReturned) = service.GenerateTokenWithExpiry(claims, explicitExp);

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Equal(expReturned, explicitExp);

            var jwtSection = cfg.GetSection("Jwt");
            var (principal, jwt) = Validate(
                token,
                key: jwtSection["Key"]!,
                issuer: jwtSection["Issuer"]!,
                audience: jwtSection["Audience"]!);

            Assert.Equal("42", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal("rodrigo", principal.FindFirstValue(ClaimTypes.Name));
            Assert.Contains("Admin", principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
            Assert.Equal(explicitExp, jwt.ValidTo); 
        }

        [Fact]
        public void GenerateToken_ShouldUseConfiguredExpiryMinutes_WhenExpiresNull()
        {
            var cfg = BuildConfig(expiresMinutes: "15");
            var service = new TokenService(cfg);

            var now = DateTime.UtcNow;
            var token = service.GenerateToken(SampleClaims(), expires: null);

            Assert.False(string.IsNullOrWhiteSpace(token));

            var jwtSection = cfg.GetSection("Jwt");
            var (_, jwt) = Validate(
                token,
                key: jwtSection["Key"]!,
                issuer: jwtSection["Issuer"]!,
                audience: jwtSection["Audience"]!);

            var delta = jwt.ValidTo - now;
            Assert.InRange(delta.TotalMinutes, 14.0, 16.0);
        }

        [Fact]
        public void GenerateToken_ShouldReturnSameTokenString_AsWriteTokenOfEmbeddedJwt()
        {
            var cfg = BuildConfig(expiresMinutes: "5");
            var service = new TokenService(cfg);

            var tokenStr = service.GenerateToken(SampleClaims());

            var jwtSection = cfg.GetSection("Jwt");
            var _ = Validate(
                tokenStr,
                key: jwtSection["Key"]!,
                issuer: jwtSection["Issuer"]!,
                audience: jwtSection["Audience"]!);

            Assert.Equal(3, tokenStr.Split('.').Length);
        }
    }
}
