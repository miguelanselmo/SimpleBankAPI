using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleBankAPI.Infrastructure.Providers;

public class AuthenticationProvider : IAuthenticationProvider
{
    private readonly ILogger<AuthenticationProvider> _logger;
    private readonly IConfiguration _config;

    private static string? _issuer;
    private static string? _audience;
    private static string? _key;
    private static int? _accessTokenDuration;
    private static int? _refreshTokenDuration;

    public AuthenticationProvider(ILogger<AuthenticationProvider> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _issuer = config["Jwt:Issuer"];
        _audience = config["Jwt:Audience"];
        _key = config["Jwt:Key"];
        _accessTokenDuration = int.Parse(config["Jwt:AccessTokenDuration"]);
        _refreshTokenDuration = int.Parse(config["Jwt:RefreshTokenDuration"]);

    }

    public (bool, string?, Session?) GetClaimSession(string authToken)
    {
        try
        {
            var resultAuth = GetToken(authToken);
            if (!resultAuth.Item1)
                return (false, "Missing bearer token.", null);
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(resultAuth.Item2);
            //Dictionary<string, string> tokenPayload = securityToken.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
            Guid? sessionId = Guid.Parse(securityToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value);
            int? userId = int.Parse(securityToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
            if (userId is null)
                return (false, "Missing User info in Token.", null);
            if (sessionId is null)
                return (false, "Missing Session info in Token.", null);

            return (true, null, new Session
            {
                Id = sessionId.Value,
                UserId = userId.Value,
                TokenAccess = resultAuth.Item2,
                TokenAccessExpireAt = securityToken.ValidTo
            });
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public (bool, string) GetToken(StringValues authToken)
    {
        string authHeader = authToken.First();
        string token = authHeader.Substring("Bearer ".Length).Trim();
        return (token is not null, token);
    }

    public Session GenerateToken(User user)
    {
        Session session = new Session
        {
            Id = Guid.NewGuid()
        };
        session = GenerateAccessToken(session, user);
        return session;// GenerateRefreshToken(session, user); //TODO: refresh token
    }

    public Session RenewToken(Session session, User user)
    {
        session.TokenRefresh = session.TokenAccess;
        session.TokenRefreshExpireAt = session.TokenRefreshExpireAt;
        return GenerateAccessToken(session, user);
    }

    private Session GenerateAccessToken(Session session, User user)
    {
        DateTime expiredAt = DateTime.UtcNow.AddMinutes((double)_accessTokenDuration);
        //Guid sessionId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Sid, session.Id.ToString())
        };
        var token = new JwtSecurityToken
        (
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiredAt,
            notBefore: DateTime.UtcNow,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256)
        );
        session.TokenAccess = new JwtSecurityTokenHandler().WriteToken(token);
        session.TokenAccessExpireAt = expiredAt;
        return session;
    }

    private Session GenerateRefreshToken(Session session, User user)
    {
        DateTime expiredAt = DateTime.UtcNow.AddMinutes((double)_refreshTokenDuration);
        //Guid sessionId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Sid, session.Id.ToString())
        };
        var token = new JwtSecurityToken
        (
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiredAt,
            notBefore: DateTime.UtcNow,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256)
        );
        session.TokenRefresh = new JwtSecurityTokenHandler().WriteToken(token);
        session.TokenRefreshExpireAt = expiredAt;
        return session;
    }
}
