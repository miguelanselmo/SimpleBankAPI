using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IAccountRepository
{
    Task<bool> Delete(int id);
    Task<AccountModel?> Read(int id);
    Task<IEnumerable<AccountModel>> Read();
    Task<bool> Create(AccountModel account);
    Task<bool> Update(AccountModel account);
}
