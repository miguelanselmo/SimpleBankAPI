using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface ITransferUseCase
{
    Task<(bool, string?, Movement?)> Transfer(Transfer transfer);

}
