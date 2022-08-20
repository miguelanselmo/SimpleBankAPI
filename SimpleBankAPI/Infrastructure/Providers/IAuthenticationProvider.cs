using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Providers;

public interface IAuthenticationProvider
{
    bool ValidateToken();
    Session GenerateToken(User user);
    (bool, User?) GetClaimUser(string token);
    (bool, Session?) GetClaimSession(string token);
    (bool, string) GetToken(StringValues authToken);
}
