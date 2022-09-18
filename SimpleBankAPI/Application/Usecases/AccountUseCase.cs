using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Ports.Repositories;

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
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating account");
            return (false, "Error creating account. Please try again.", null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(bool, string?, IEnumerable<Account>)> GetAccounts(int userId)
    {
        try
        {
            var result = await _unitOfWork.AccountRepository.ReadByUser(userId);
            if (result is not null)
                return (true, null, result);
            else
                return (false, "Accounts not found.", null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting accounts");
            return (false, "Error getting accounts", null);
        }
    }

    public async Task<(bool, string?, Account, IEnumerable<Movement>)> GetAccountMovements(int userId, int id)
    {
        try
        {
            var result = await _unitOfWork.AccountRepository.ReadById(userId, id);
            if (result is not null)
            {
                return (true, null, result, await _unitOfWork.MovementRepository.ReadByAccount(id));
            }
            else
                return (false, "Account not found.", null, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting account movements");
            return (false, "Error getting account movements", null, null);
        }
    }
}
