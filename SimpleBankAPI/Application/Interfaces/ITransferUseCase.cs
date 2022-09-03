using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Application.Interfaces;

public interface ITransferUseCase
{
    Task<(bool, string?, Movement?)> Transfer(Transfer transfer);

}
