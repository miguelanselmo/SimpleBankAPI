using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public interface IUserUseCase
{
    Task<(bool, string?, UserModel?)> CreateUser(UserModel user);

    Task<(bool, string?, UserModel?, SessionModel?)> Login(UserModel user);

}
