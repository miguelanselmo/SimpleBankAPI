using SimpleBankAPI.Models;

namespace SimpleBankAPI.Repositories;

public interface ITransferRepository
{
    Task<bool> Delete(int id);
    Task<TransferModel?> Read(int id);
    Task<IEnumerable<TransferModel>> Read();
    Task<bool> Create(TransferModel transfer);
    Task<bool> Update(TransferModel transfer);
}
