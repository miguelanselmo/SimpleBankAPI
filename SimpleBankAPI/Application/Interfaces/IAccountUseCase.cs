
namespace SimpleBankAPI.Application.Interfaces;

public interface IAccountUseCase
{
    Task<(ErrorTypeUsecase?, string?, Account?)> CreateAccount(Account user);

    Task<(ErrorTypeUsecase?, string?, IEnumerable<Account>?)> GetAccounts(int userId);

    Task<(ErrorTypeUsecase?, string?, Account?, IEnumerable<Movement>?)> GetAccountMovements(int userId, int id);
}
