using Microsoft.Extensions.Primitives;
using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Ports.Providers;

public interface IAuthenticationProvider
{
    Session GenerateToken(User user);
    (bool, string?, Session?) GetClaims(string authToken);
    (bool, string) GetToken(StringValues authToken);
    Session RenewToken(User user, Session session);
}
