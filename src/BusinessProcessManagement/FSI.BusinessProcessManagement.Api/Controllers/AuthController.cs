using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using FSI.BusinessProcessManagement.Api.Models.Auth;
using FSI.BusinessProcessManagement.Api.Services;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(IUnitOfWork uow, ITokenService tokenService, IConfiguration config)
        {
            _uow = uow;
            _tokenService = tokenService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized("Credenciais inválidas.");

            var user = await _uow.Users.GetByUsernameAsync(request.Username.Trim());
            if (user == null || !user.IsActive)
                return Unauthorized("Usuário inválido ou inativo.");

            // Verifica senha (BCrypt)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Usuário ou senha inválidos.");

            // Carrega roles do usuário (ideal: método dedicado no repo)
            var roleNames = await _uow.Users.GetRoleNamesAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            foreach (var role in roleNames)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpirationMinutes"]!));
            var jwt = _tokenService.GenerateToken(claims, expiresAt);

            return Ok(new LoginResponse
            {
                AccessToken = jwt,
                ExpiresAtUtc = expiresAt,
                UserId = user.Id,
                Username = user.Username,
                Roles = roleNames
            });
        }
    }
}
