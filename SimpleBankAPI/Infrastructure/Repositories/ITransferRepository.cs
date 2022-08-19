using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Infrastructure.Repositories;

public interface ITransferRepository
{
    Task<IEnumerable<TransferModel>> ReadByAccount(int accountId);
    Task<IEnumerable<TransferModel>> ReadAll();
    Task<bool> Create(TransferModel dataModel);
    Task<bool> Update(TransferModel dataModel);
    Task<bool> Delete(int id);
}
