using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Ports.Providers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SimpleBankAPI.Infrastructure.Adapters.Providers;

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

    public (bool, string?, Session?) GetClaims(string authToken)
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
        session = GenerateRefreshToken(session);
        return session;
    }

    public Session RenewToken(User user, Session session)
    {
        session = GenerateAccessToken(session, user);
        //session = GenerateRefreshToken(session);
        return session;
    }

    private static Session GenerateAccessToken(Session session, User user)
    {
        session.CreatedAt = DateTime.UtcNow;
        session.TokenAccessExpireAt = DateTime.UtcNow.AddMinutes((double)_accessTokenDuration);
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
            expires: session.TokenAccessExpireAt,
            notBefore: session.CreatedAt,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256)
        );
        session.TokenAccess = new JwtSecurityTokenHandler().WriteToken(token);
        return session;
    }

    private static Session GenerateRefreshToken(Session session)
    {
        var randomNumber = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            session.TokenRefresh = Convert.ToBase64String(randomNumber);
            session.TokenRefreshExpireAt = DateTime.UtcNow.AddMinutes((double)_refreshTokenDuration);
            return session;
        }
    }
}
