using AutoMapper;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using System.Security.Claims;

namespace CryptoOnRamp.BLL.Services;

public class AuthenticationService(
    IJwtTokenGenerator jwtTokenGenerator,
    ISessionRepository sessionRepository,
    IMapper mapper) : IAuthenticationService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<AuthenticationContext> GenerateAuthenticationContextAsync(string userId, IEnumerable<Claim> claims, TimeSpan accessTokenExpiry, TimeSpan refreshTokenExpiry, User userClientInfo, CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid();
        var nonce = 0L;


        var context = await GenerateAuthenticationContextAsync(
        userId: userId,
            session: new Session { UserId = userId, Id = sessionId },
            nonce: nonce,
            claims: claims,
            accessTokenExpiry: accessTokenExpiry,
            refreshTokenExpiry: refreshTokenExpiry,
            userClientInfo: userClientInfo,
            cancellationToken: cancellationToken);

        return context;
    }

    private async Task<AuthenticationContext> GenerateAuthenticationContextAsync(
string userId,
Session session,
long nonce,
IEnumerable<Claim> claims,
TimeSpan accessTokenExpiry,
TimeSpan refreshTokenExpiry,
User userClientInfo,
CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiredAt = now + accessTokenExpiry;
        var refreshTokenExpiredAt = now + refreshTokenExpiry;

        var accessToken = _jwtTokenGenerator.GenerateJwtToken(GetClaims(userId, session.Id, nonce, claims, AuthenticationTokenType.Access), accessTokenExpiredAt);
        var refreshToken = _jwtTokenGenerator.GenerateJwtToken(GetClaims(userId, session.Id, nonce, claims, AuthenticationTokenType.Refresh), refreshTokenExpiredAt);

        session.Nonce = nonce;
        session.Claims = GetUserClaims(claims).ToDictionary(c => c.Type, c => c.Value);
        session.ActivityAt = now;
        session.ExpiredAt = refreshTokenExpiredAt;

        var sessionDb = _mapper.Map<SessionDb>(session);

        await _sessionRepository.InsertAsync(sessionDb);
        await _sessionRepository.SaveAsync();

        var context = new AuthenticationContext
        {
            Id = userId,
            AccessToken = accessToken,
            AccessTokenExpiredAt = accessTokenExpiredAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiredAt = refreshTokenExpiredAt
        };

        return context;
    }

    private static IEnumerable<Claim> GetClaims(string userId, Guid sessionId, long nonce, IEnumerable<Claim> claims, AuthenticationTokenType tokenType)
   => new List<Claim>(
       collection: GetUserClaims(claims))
   {
        new(AppClaims.Id, userId.ToString()),
        new(AppClaims.Session, sessionId.ToString()),
        new(AppClaims.Nonce, nonce.ToString()),
        new(AppClaims.TokenType, tokenType.ToString()),
   };

    private static IEnumerable<Claim> GetUserClaims(IEnumerable<Claim> claims)
        => claims
            .Where(claim => claim.Type.StartsWith(AppClaims.ClaimPrefix))
            .Where(claim => claim.Type != AppClaims.Id)
            .Where(claim => claim.Type != AppClaims.Session)
            .Where(claim => claim.Type != AppClaims.Nonce)
            .Where(claim => claim.Type != AppClaims.Otc)
            .Where(claim => claim.Type != AppClaims.TokenType);

    public async Task<AuthenticationContext> RefreshAuthenticationContextAsync(string refreshToken, TimeSpan accessTokenExpiry, TimeSpan refreshTokenExpiry, User userClientInfo, CancellationToken cancellationToken)
    {
        var jwt = _jwtTokenGenerator.ParseJwtToken(refreshToken);

        var userId = GetUserId(jwt.Claims);
        var sessionId = GetSessionId(jwt.Claims);
        var nonce = GetNonce(jwt.Claims);

        if (userId != userClientInfo.Id.ToString())
        {
            throw new Exception("Wrong User Id");
        }

        var sessionDb = await _sessionRepository.GetFirstOrDefaultAsync(x => x.UserId == userId && x.Id == sessionId);
        if (sessionDb is null || sessionDb.Nonce != nonce)
            throw new Exception("Unauthorized");

        _sessionRepository.Delete(sessionDb);
        await _sessionRepository.SaveAsync();

        var session = _mapper.Map<Session>(sessionDb);

        var context = await GenerateAuthenticationContextAsync(
            userId: userId,
            session: session,
            nonce: nonce + 1,
            claims: jwt.Claims,
            accessTokenExpiry: accessTokenExpiry,
            refreshTokenExpiry: refreshTokenExpiry,
            userClientInfo: userClientInfo,
            cancellationToken: cancellationToken);

        return context;
    }

    private static string GetUserId(IEnumerable<Claim> claims)
    {
        var result = GetRequiredClaim(claims, AppClaims.Id).Value;
        if (string.IsNullOrEmpty(result))
        {
            throw new Exception();
        }

        return result;
    }

    private static Guid GetSessionId(IEnumerable<Claim> claims)
        => Guid.TryParse(GetRequiredClaim(claims, AppClaims.Session).Value, out var sessionId) ? sessionId : throw new Exception();

    private static long GetNonce(IEnumerable<Claim> claims)
        => long.TryParse(GetRequiredClaim(claims, AppClaims.Nonce).Value, out var nonce) ? nonce : throw new Exception();

    private static Claim GetRequiredClaim(IEnumerable<Claim> claims, string name)
=> claims.FirstOrDefault(claim => claim.Type == name) ?? throw new Exception();

}