using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FSI.BusinessProcessManagement.Api.Services;
using FSI.BusinessProcessManagement.Api.Models.Auth;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IRepository<User> _users;
        private readonly IRepository<UserRole> _userRoles;
        private readonly IRepository<Role> _roles;
        private readonly ITokenService _tokenService;

        public AuthController(
            IRepository<User> users,
            IRepository<UserRole> userRoles,
            IRepository<Role> roles,
            ITokenService tokenService)
        {
            _users = users;
            _userRoles = userRoles;
            _roles = roles;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            // 1) Localiza usuário por Username
            var allUsers = await _users.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Username == request.Username);
            if (user is null) return Unauthorized();

            if (!user.IsActive) return Unauthorized();

            // 2) Valida senha (BCrypt recomendado)
            // Se PasswordHash já está em BCrypt, use Verify; se for legado em texto, faça fallback
            var stored = user.PasswordHash ?? string.Empty;
            var ok =
                (stored.StartsWith("$2a$") || stored.StartsWith("$2b$") || stored.StartsWith("$2y$"))
                    ? BCrypt.Net.BCrypt.Verify(request.Password, stored)
                    : stored == request.Password; // fallback legado (troque assim que migrar tudo pra BCrypt)

            if (!ok) return Unauthorized();

            // 3) Busca roles do usuário
            var allUserRoles = await _userRoles.GetAllAsync();
            var userRoleIds = allUserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToHashSet();

            var allRoles = await _roles.GetAllAsync();
            var roleNames = allRoles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();

            // 4) Monta claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            claims.AddRange(roleNames.Select(rn => new Claim(ClaimTypes.Role, rn)));

            // 5) Gera token
            var (token, expiresAtUtc) = _tokenService.GenerateTokenWithExpiry(claims);

            // 6) Retorno no seu modelo
            var resp = new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAtUtc,
                UserId = user.Id,
                Username = user.Username,
                Roles = roleNames
            };

            return Ok(resp);
        }
    }
}
