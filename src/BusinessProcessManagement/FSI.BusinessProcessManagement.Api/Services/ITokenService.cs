using System.Security.Claims;

namespace FSI.BusinessProcessManagement.Api.Services
{
    public interface ITokenService
    {
        // Método existente (mantido para compatibilidade)
        string GenerateToken(IEnumerable<Claim> claims, DateTime? expires = null);

        // Overload que retorna também o ExpiresAtUtc (usado pelo LoginResponse)
        (string token, DateTime ExpiresAtUtc) GenerateTokenWithExpiry(IEnumerable<Claim> claims, DateTime? expires = null);
    }
}
