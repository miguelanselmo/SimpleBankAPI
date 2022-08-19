using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Providers;

public interface IAuthenticationProvider
{
    bool ValidateToken();
    SessionModel GenerateToken(UserModel user);
    (bool, UserModel?) GetClaimUser(string token);
    (bool, string) GetToken(StringValues authToken);
}
