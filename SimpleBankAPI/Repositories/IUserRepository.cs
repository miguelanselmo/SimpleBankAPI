using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IUserRepository
{
    Task<bool> Delete(int id);
    Task<UserModel?> Read(int id);
    Task<IEnumerable<UserModel>> Read();
    Task<bool> Create(UserModel issue);
    Task<bool> Update(UserModel issue);
}
