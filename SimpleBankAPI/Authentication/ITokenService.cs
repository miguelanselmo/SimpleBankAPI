using SimpleBankAPI.Models;

namespace SimpleBankAPI.Authentication
{
    public interface ITokenService
    {
        string GenerateToken(string key, string issuer, string audience, UserModel user);
    }
}
