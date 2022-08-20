using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface ITransferRepository
{
    Task<IEnumerable<Transfer>> ReadByAccount(int accountId);
    Task<IEnumerable<Transfer>> ReadAll();
    Task<bool> Create(Transfer data);
    Task<bool> Update(Transfer data);
    Task<bool> Delete(int id);
}
