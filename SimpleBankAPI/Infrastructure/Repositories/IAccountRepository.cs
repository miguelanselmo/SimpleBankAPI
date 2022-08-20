using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface IAccountRepository
{
    Task<Account?> ReadById(int userId, int id);
    Task<Account?> ReadById(int id);
    Task<IEnumerable<Account?>> ReadByUser(int userId);
    Task<IEnumerable<Account>> ReadAll();
    Task<(bool, int?)> Create(Account data);
    Task<bool> Update(Account data);
    Task<bool> Delete(int id);
}
