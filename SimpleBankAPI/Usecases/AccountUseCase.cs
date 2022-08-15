using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;

namespace SimpleBankAPI.Usecases;

public class AccountUseCase : IAccountUseCase
{
    private readonly ILogger<AccountUseCase> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IMovementRepository _movementRepository;
    public AccountUseCase(ILogger<AccountUseCase> logger, IAccountRepository accountRepository, ITransferRepository transferRepository, IMovementRepository movementRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _movementRepository = movementRepository;
    }
    
    public async Task<(bool, string?, AccountModel?)> CreateAccount(AccountModel account)
    {
        var result = await _accountRepository.Create(account);
        if (result.Item1)
        {
            account.Id = (int)result.Item2;
            return (true, null, account);
        }
        else
            return (false, "Account not created. Please try again.", null);
    }

    public async Task<(bool, string?, IEnumerable<AccountModel>)> GetAccounts(int userId)
    {
        var result = await _accountRepository.ReadByUserId(userId);
        if (result is not null)
            return (true, null, result);
        else
            return (false, "Accounts not found.", null);

    }
    
    public async Task<(bool, string?, AccountModel, IEnumerable<MovementModel>)> GetAccountDetails(int id)
    {
        var result = await _accountRepository.ReadById(id);
        if (result is not null)
        {
            return (true, null, result, await _movementRepository.ReadByAccount(id));
        }
        else
            return (false, "Account not found.", null, null);
    }
    
    public async Task<(bool, string?)> Transfer(TransferModel transfer)
    {
        return (true, null);
    }    

}
