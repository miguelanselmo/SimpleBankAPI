using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Models;

namespace SimpleBankAPI.Providers;

public interface IAuthenticationProvider
{
    bool ValidateToken();
    SessionModel GenerateToken(UserModel user);
    (bool, UserModel?) GetClaimUser(string token);
    (bool, string) GetToken(StringValues authToken);
}
