using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories;

namespace SimpleBankAPI.Application.Usecases;

public class AccountUseCase : IAccountUseCase
{
    private readonly ILogger<AccountUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AccountUseCase(ILogger<AccountUseCase> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool, string?, Account?)> CreateAccount(Account account)
    {
        bool commit = false;
        try
        {
            var result = await _unitOfWork.AccountRepository.Create(account);
            if (result.Item1)
            {
                commit = true;
                account.Id = (int)result.Item2;
                return (true, null, account);
            }
            else
                return (result.Item1, "Account not created. Please try again.", null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(bool, string?, IEnumerable<Account>)> GetAccounts(int userId)
    {
        var result = await _unitOfWork.AccountRepository.ReadByUser(userId);
        if (result is not null)
            return (true, null, result);
        else
            return (false, "Accounts not found.", null);
    }

    public async Task<(bool, string?, Account, IEnumerable<Movement>)> GetAccountMovements(int userId, int id)
    {
        var result = await _unitOfWork.AccountRepository.ReadById(userId, id);
        if (result is not null)
        {
            return (true, null, result, await _unitOfWork.MovementRepository.ReadByAccount(id));
        }
        else
            return (false, "Account not found.", null, null);
    }
}
