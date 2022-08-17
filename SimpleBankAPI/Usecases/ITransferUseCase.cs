using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public interface ITransferUseCase
{
    Task<(bool, string?)> Transfer(TransferModel transfer);

}
