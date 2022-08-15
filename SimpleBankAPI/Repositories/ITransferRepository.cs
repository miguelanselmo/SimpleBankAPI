using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface ITransferRepository
{
    Task<bool> Delete(int id);
    Task<TransferModel?> ReadById(int id);
    Task<IEnumerable<TransferModel>> ReadAll();
    Task<bool> Create(TransferModel dataModel);
    Task<bool> Update(TransferModel dataModel);
}
