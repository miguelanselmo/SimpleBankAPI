using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Application.Interfaces;

public interface ISessionUseCase
{
    Task<(bool, string?, User?, Session?)> Login(User user);
    Task<(bool, string?, Session?)> Logout(Session session);
    Task<(bool, string?, Session?)> CheckSession(Session session);
    Task<(bool, string?, User?, Session?)> RenewLogin(Session session, string refreshToken);

}
