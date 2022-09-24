
namespace SimpleBankAPI.Application.Interfaces;

public interface ITransferUseCase
{
    Task<(ErrorTypeUsecase?, string?, Movement?)> Transfer(Transfer transfer);

}
