using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface IUserUseCase
{
    Task<(bool, string?, User?)> CreateUser(User user);
    
    Task<(bool, string?, User?, Session?)> Login(User user);
    Task<(bool, string?, Session?)> Logout(Session session);

}
