using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface IAccountUseCase
{
    Task<(bool, string?, Account?)> CreateAccount(Account user);
    
    Task<(bool, string?, IEnumerable<Account>)> GetAccounts(int userId);

    Task<(bool, string?, Account, IEnumerable<Movement>)> GetAccountMovements(int userId, int id);
}
