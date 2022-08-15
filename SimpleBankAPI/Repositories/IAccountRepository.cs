using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface IAccountRepository
{
    Task<bool> Delete(int id);
    Task<AccountModel?> ReadById(int id);
    Task<IEnumerable<AccountModel?>> ReadByUserId(int userId);
    Task<IEnumerable<AccountModel>> ReadAll();
    Task<(bool, int?)> Create(AccountModel dataModel);
    Task<bool> Update(AccountModel dataModel);
}
