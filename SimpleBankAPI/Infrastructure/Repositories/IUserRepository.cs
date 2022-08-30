using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User?> ReadById(int id);
    Task<User?> ReadByName(string name);
    Task<IEnumerable<User>?> ReadAll();
    Task<(bool, int?)> Create(User data);
    Task<bool> Update(User dataModel);
    // Task<bool> Delete(int id);
}
