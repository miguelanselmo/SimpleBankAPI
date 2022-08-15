using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public interface IUserUseCase
{
    public Task<(bool, string?, UserModel?)> CreateUser(UserModel user);

    public Task<(bool, string?)> Login(UserModel user);

}
