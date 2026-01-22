using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ERPApi.Core.Entities;

namespace ERPApi.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user, List<string> roles, List<string> permissions);
        string GenerateRefreshToken();
        DateTime GetRefreshTokenExpiryTime();
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromToken(string token);
        JwtSecurityToken ReadToken(string token);
    }
}