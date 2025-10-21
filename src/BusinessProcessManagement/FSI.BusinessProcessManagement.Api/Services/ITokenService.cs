using System.Security.Claims;

namespace FSI.BusinessProcessManagement.Api.Services
{
    public interface ITokenService
    {
        string GenerateToken(IEnumerable<Claim> claims, DateTime? expires = null);
    }
}
