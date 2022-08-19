using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories;

namespace SimpleBankAPI.Core.Usecases;

public class AccountUseCase : IAccountUseCase
{
    private readonly ILogger<AccountUseCase> logger;
    private readonly IUnitOfWork unitOfWork;
    public AccountUseCase(ILogger<AccountUseCase> logger, IUnitOfWork unitOfWork)
    {
        this.logger = logger;
        this.unitOfWork = unitOfWork;
    }
    
    public async Task<(bool, string?, AccountModel?)> CreateAccount(AccountModel account)
    {
        var result = await unitOfWork.AccountRepository.Create(account);
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
        var result = await unitOfWork.AccountRepository.ReadByUser(userId);
        if (result is not null)
            return (true, null, result);
        else
            return (false, "Accounts not found.", null);
    }
    
    public async Task<(bool, string?, AccountModel, IEnumerable<MovementModel>)> GetAccountMovements(int userId, int id)
    {
        var result = await unitOfWork.AccountRepository.ReadById(userId, id);
        if (result is not null)
        {
            return (true, null, result, await unitOfWork.MovementRepository.ReadByAccount(id));
        }
        else
            return (false, "Account not found.", null, null);
    }
}
