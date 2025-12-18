using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CryptoOnRamp.BLL.Services;

internal class JwtTokenGenerator(IOptions<AuthenticationSettings> options) : IJwtTokenGenerator
{
    private readonly AuthenticationSettings _options = options.Value;

    public string GenerateJwtToken(IEnumerable<Claim> claims, DateTime expiredAt)
    {

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.PrivateKey));

        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokeOptions = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiredAt,
            signingCredentials: signinCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

        return token;
    }

    public JwtSecurityToken ParseJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
            throw new Exception("jwtTokenCannotBeRead");

        var jwt = handler.ReadToken(token) as JwtSecurityToken;
        if (jwt is null)
            throw new Exception("JwtTokenIsNotSecurityToken");

        return jwt;
    }
}
