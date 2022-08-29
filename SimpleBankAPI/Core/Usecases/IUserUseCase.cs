using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface IUserUseCase
{
    Task<(bool, string?, User?)> CreateUser(User user);
}
