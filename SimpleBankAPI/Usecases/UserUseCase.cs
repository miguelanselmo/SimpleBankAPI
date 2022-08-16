using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleBankAPI.Usecases;

public class UserUseCase : IUserUseCase
{
    private readonly ILogger<UserUseCase> _logger;
    private readonly IUserRepository _repository;
    private readonly IConfiguration _config;

    private static string? _issuer;
    private static string? _audience;
    private static string? _key;
    private static int? _accessTokenDuration;
    private static int? _refreshTokenDuration;
    
    public UserUseCase(ILogger<UserUseCase> logger, IUserRepository repository, IConfiguration config)
    {
        _logger = logger;
        _repository = repository;
        _issuer = config["Jwt:Issuer"];
        _audience = config["Jwt:Audience"];
        _key = config["Jwt:Key"];
        _accessTokenDuration = int.Parse(config["Jwt:AccessTokenDuration"]);
        _refreshTokenDuration = int.Parse(config["Jwt:RefreshTokenDuration"]);
    }

    public async Task<(bool,string?, UserModel?)> CreateUser(UserModel user)
    {
        var userDb = await _repository.ReadByUsername(user.UserName);
        if (userDb is null)
        {
            var result = await _repository.Create(user);
            if (result.Item1)
            {
                user.Id = (int)result.Item2;
                return (true, null, user);
            }
            else
                return (false, "User not created. Please try again.", null);
        }
        else
            return (false, "Username already exists", null);
    }
    
    public async Task<(bool,string?,UserModel?,SessionModel?)> Login(UserModel user)
    {
        var userDb = await _repository.ReadByUsername(user.UserName);
        if (userDb is not null)
        {
            if (userDb.Password.Equals(user.Password))
            {
                var token = GenerateToken(user);
                return (true, null, userDb, new SessionModel { 
                    TokenAccess = token.Item1, 
                    TokenAccessExpireAt = token.Item2,
                    SessionId = Guid.NewGuid() });
            }
            else
                return (false, "Invalid authentication", null, null);
        }
        else
        {
            return (false, "User not found", null, null);
        }
    }

    public static (string,DateTime) GenerateToken(UserModel user)
    {
        DateTime expiredAt = DateTime.UtcNow.AddMinutes((double)_accessTokenDuration);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            //new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
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
        return (new JwtSecurityTokenHandler().WriteToken(token), expiredAt);
    }
}
