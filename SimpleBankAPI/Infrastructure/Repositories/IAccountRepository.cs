using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface IAccountRepository
{
    Task<AccountModel?> ReadById(int userId, int id);
    Task<AccountModel?> ReadById(int id);
    Task<IEnumerable<AccountModel?>> ReadByUser(int userId);
    Task<IEnumerable<AccountModel>> ReadAll();
    Task<(bool, int?)> Create(AccountModel dataModel);
    Task<bool> Update(AccountModel dataModel);
    Task<bool> Delete(int id);
}
