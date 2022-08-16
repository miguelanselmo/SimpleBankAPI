using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public interface ITransferUseCase
{
    public Task<(bool, string?, TransferModel?)> Transfer(TransferModel transfer);

}
