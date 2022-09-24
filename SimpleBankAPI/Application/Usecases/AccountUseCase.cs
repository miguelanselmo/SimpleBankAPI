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

    public async Task<(ErrorTypeUsecase?, string?, Account?)> CreateAccount(Account account)
    {
        bool commit = false;
        try
        {
            var result = await _unitOfWork.AccountRepository.Create(account);
            if (result.Item1)
            {
                commit = true;
                account.Id = (int)result.Item2;
                return (null, null, account);
            }
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError), null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(ErrorTypeUsecase?, string?, IEnumerable<Account>?)> GetAccounts(int userId)
    {
        try
        {
            var result = await _unitOfWork.AccountRepository.ReadByUser(userId);
            if (result is not null)
                return (null, null, result);
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.AccountNotFound), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountReadError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.AccountReadError), null);
        }
    }

    public async Task<(ErrorTypeUsecase?, string?, Account?, IEnumerable<Movement>?)> GetAccountMovements(int userId, int id)
    {
        try
        {
            var result = await _unitOfWork.AccountRepository.ReadById(userId, id);
            if (result is not null)
            {
                return (null, null, result, await _unitOfWork.MovementRepository.ReadByAccount(id));
            }
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.AccountNotFound), null, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountMovementReadError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.AccountMovementReadError), null, null);
        }
    }
}
