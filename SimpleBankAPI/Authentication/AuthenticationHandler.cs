using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using SimpleBankAPI.Models;

namespace SimpleBankAPI.Authentication
{
    public class AuthenticationHandler
    {
        private static string? issuer;
        private static string? audience;
        private static string? key;
        private static int? accessTokenDuration;
        private static int? refreshTokenDuration;
        public static void Configure(IConfiguration config)
        {
            issuer = config["Jwt:Issuer"];
            audience = config["Jwt:Audience"];
            key = config["Jwt:Key"];
            accessTokenDuration = int.Parse(config["Jwt:AccessTokenDuration"]);
            refreshTokenDuration = int.Parse(config["Jwt:RefreshTokenDuration"]);
        }

        public static string GenerateToken()
        {
            /*
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, loggedInUser.Username),
            new Claim(ClaimTypes.Email, loggedInUser.EmailAddress),
            new Claim(ClaimTypes.GivenName, loggedInUser.GivenName),
            new Claim(ClaimTypes.Surname, loggedInUser.Surname),
            new Claim(ClaimTypes.Role, loggedInUser.Role)
            };
            */
            var token = new JwtSecurityToken
            (
                issuer: issuer,
                audience: audience,
                //claims: claims,
                expires: DateTime.UtcNow.AddDays(60),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateToken(string userName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
                /*new Claim(ClaimTypes.Expiration, userName),
                new Claim(ClaimTypes.Email, userName),
                new Claim(ClaimTypes., userName),*/
                new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes((double)accessTokenDuration),
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }
    }

}
