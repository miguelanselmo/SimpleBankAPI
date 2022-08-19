using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface ITransferUseCase
{
    Task<(bool, string?)> Transfer(TransferModel transfer);

}
