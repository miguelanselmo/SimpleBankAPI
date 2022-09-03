using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Application.Interfaces;

public interface IUserUseCase
{
    Task<(bool, string?, User?)> CreateUser(User user);
}
