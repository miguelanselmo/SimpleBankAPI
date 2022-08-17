using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleBankAPI.Providers;

public class AuthenticationProvider : IAuthenticationProvider
{
    private readonly ILogger<AuthenticationProvider> logger;
    private readonly IConfiguration config;

    private static string? issuer;
    private static string? audience;
    private static string? key;
    private static int? accessTokenDuration;
    private static int? refreshTokenDuration;

    public AuthenticationProvider(ILogger<AuthenticationProvider> logger, IConfiguration config)
    {
        this.logger = logger;
        this.config = config;
        issuer = config["Jwt:Issuer"];
        audience = config["Jwt:Audience"];
        key = config["Jwt:Key"];
        accessTokenDuration = int.Parse(config["Jwt:AccessTokenDuration"]);
    }

    public (bool, UserModel?) GetClaimUser(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            return (true, new UserModel
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

    public SessionModel GenerateToken(UserModel user)
    {
        DateTime expiredAt = DateTime.UtcNow.AddMinutes((double)accessTokenDuration);
        Guid sessionId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            //new Claim(ClaimTypes.Id, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken
        (
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiredAt,
            notBefore: DateTime.UtcNow,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
        );
        return new SessionModel
        {
            TokenAccess = new JwtSecurityTokenHandler().WriteToken(token),
            TokenAccessExpireAt = expiredAt,
            SessionId = sessionId
        };
    }
}
