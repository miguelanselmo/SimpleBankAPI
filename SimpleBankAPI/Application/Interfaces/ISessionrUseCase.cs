
namespace SimpleBankAPI.Application.Interfaces;

public interface ISessionUseCase
{
    Task<(ErrorTypeUsecase?, string?, User?, Session?)> Login(User user);
    Task<(ErrorTypeUsecase?, string?, Session?)> Logout(Session session);
    Task<(ErrorTypeUsecase?, string?, Session?)> CheckSession(Session session);
    Task<(ErrorTypeUsecase?, string?, User?, Session?)> RenewLogin(Session session, string refreshToken);

}
