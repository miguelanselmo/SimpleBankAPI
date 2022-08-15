using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public interface IAccountUseCase
{
    public Task<(bool, string?, AccountModel?)> CreateAccount(AccountModel user);
    
    public Task<(bool, string?, IEnumerable<AccountModel>)> GetAccounts(int userId);

    public Task<(bool, string?, AccountModel, IEnumerable<MovementModel>)> GetAccountDetails(int id);

    public Task<(bool, string?)> Transfer(TransferModel transfer);

}
