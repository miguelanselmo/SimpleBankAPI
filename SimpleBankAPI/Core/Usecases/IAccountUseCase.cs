using SimpleBankAPI.Core.Entities;

namespace SimpleBankAPI.Core.Usecases;

public interface IAccountUseCase
{
    Task<(bool, string?, AccountModel?)> CreateAccount(AccountModel user);
    
    Task<(bool, string?, IEnumerable<AccountModel>)> GetAccounts(int userId);

    Task<(bool, string?, AccountModel, IEnumerable<MovementModel>)> GetAccountMovements(int userId, int id);
}
