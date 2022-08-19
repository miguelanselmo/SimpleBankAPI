using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface IUserUseCase
{
    Task<(bool, string?, UserModel?)> CreateUser(UserModel user);

    Task<(bool, string?, UserModel?, SessionModel?)> Login(UserModel user);

}
