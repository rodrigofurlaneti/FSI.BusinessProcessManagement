using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FSI.BusinessProcessManagement.Api.Models.Auth;
using FSI.BusinessProcessManagement.Api.Services;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using BCrypt.Net;

namespace FSI.BusinessProcessManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;

        public AuthController(IUnitOfWork uow, ITokenService tokenService)
        {
            _uow = uow;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized("Credenciais inválidas.");

            var user = await _uow.Users.GetByUsernameAsync(request.Username.Trim());
            if (user == null || !user.IsActive)
                return Unauthorized("Usuário inválido ou inativo.");

            bool ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!ok)
                return Unauthorized("Usuário ou senha inválidos.");

            var userRoles = await _uow.UserRoles.GetAllAsync();

            var roles = (await _uow.Users.GetRoleNamesAsync(user.Id)).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Jwt:ExpirationMinutes"]!));

            var token = _tokenService.GenerateToken(claims, expiresAt);

            var response = new LoginResponse
            {
                AccessToken = token,
                ExpiresAtUtc = expiresAt,
                UserId = user.Id,
                Username = user.Username,
                Roles = roles
            };

            return Ok(response);
        }
    }
}
