using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Providers;

public interface IAuthenticationProvider
{
    //bool ValidateToken();
    Session GenerateToken(User user);
    Session RenewToken(Session session, User user);
    //(bool, User?) GetClaimUser(string token);
    (bool, string?, Session?) GetClaimSession(string authToken);
    (bool, string) GetToken(StringValues authToken);
}
