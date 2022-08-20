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
    }

    public (bool, User?) GetClaimUser(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            return (true, new User
            {
                UserName = securityToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value,
                Id = int.Parse(securityToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value)
            });
        }
        catch (Exception ex)
        {
            return (false, null);
        }
    }

    public (bool, Session?) GetClaimSession(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            return (true, new Session
            {
                Id = Guid.Parse(securityToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value)
            });
        }
        catch (Exception ex)
        {
            return (false, null);
        }
    }

    public bool ValidateToken()
    {
        return true;
    }

    public (bool,string) GetToken(StringValues authToken)
    {
        string authHeader = authToken.First();
        string token = authHeader.Substring("Bearer ".Length).Trim();
        return (token is not null, token);
    }

    public Session GenerateToken(User user)
    {
        DateTime expiredAt = DateTime.UtcNow.AddMinutes((double)_accessTokenDuration);
        Guid sessionId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Sid, sessionId.ToString())
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
        return new Session
        {
            TokenAccess = new JwtSecurityTokenHandler().WriteToken(token),
            TokenAccessExpireAt = expiredAt,
            Id = sessionId
        };
    }
}
