using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(IEnumerable<Claim> claims, DateTime expiredAt);
    JwtSecurityToken ParseJwtToken(string token);
}
