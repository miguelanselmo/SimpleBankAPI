using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public class AccountUseCase : IAccountUseCase
{
    private readonly ILogger<AccountUseCase> logger;
    private readonly IAccountRepository accountRepository;
    private readonly IMovementRepository movementRepository;
    public AccountUseCase(ILogger<AccountUseCase> logger, IAccountRepository accountRepository, IMovementRepository movementRepository)
    {
        this.logger = logger;
        this.accountRepository = accountRepository;
        this.movementRepository = movementRepository;
    }
    
    public async Task<(bool, string?, AccountModel?)> CreateAccount(AccountModel account)
    {
        var result = await accountRepository.Create(account);
        if (result.Item1)
        {
            account.Id = (int)result.Item2;
            return (true, null, account);
        }
        else
            return (result.Item1, "Account not created. Please try again.", null);
    }

    public async Task<(bool, string?, IEnumerable<AccountModel>)> GetAccounts(int userId)
    {
        var result = await accountRepository.ReadByUser(userId);
        if (result is not null)
            return (true, null, result);
        else
            return (false, "Accounts not found.", null);
    }
    
    public async Task<(bool, string?, AccountModel, IEnumerable<MovementModel>)> GetAccountMovements(int userId, int id)
    {
        var result = await accountRepository.ReadById(userId, id);
        if (result is not null)
        {
            return (true, null, result, await movementRepository.ReadByAccount(id));
        }
        else
            return (false, "Account not found.", null, null);
    }
}
