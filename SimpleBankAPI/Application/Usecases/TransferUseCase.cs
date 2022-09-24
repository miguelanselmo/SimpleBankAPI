using SimpleBankAPI.Application.Enums;
using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Ports.Repositories;

namespace SimpleBankAPI.Application.Usecases;

public class TransferUseCase : ITransferUseCase
{
    private readonly ILogger<TransferUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public TransferUseCase(ILogger<TransferUseCase> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<(ErrorTypeUsecase?, string?, Movement?)> Transfer(Transfer transfer)
    {
        bool commit = true;
        try
        {
            _unitOfWork.Begin();
            if (transfer.ToAccountId == transfer.FromAccountId)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferSameAccount), null);
            var resultFrom = await _unitOfWork.AccountRepository.ReadById(transfer.UserId, transfer.FromAccountId);
            if (resultFrom is null)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferAccountNotFound), null);
            if (transfer.Amount > resultFrom.Balance)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferBalanceBelowAmount), null);
            var resultTo = await _unitOfWork.AccountRepository.ReadById(transfer.ToAccountId);
            if (resultTo is null)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferDestinationAccountNotFound), null);
            if (resultFrom.Currency != resultTo.Currency)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferDifferentCurrencies), null);

            var movementTo = new Movement
            {
                AccountId = transfer.ToAccountId,
                Amount = transfer.Amount,
                Balance = resultTo.Balance + transfer.Amount,
            };
            var resultMov = await _unitOfWork.MovementRepository.Create(movementTo);
            commit = resultMov.Item1;
            if (!resultMov.Item1)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferError), null);
            var movementFrom = new Movement
            {
                AccountId = transfer.FromAccountId,
                Amount = transfer.Amount * -1,
                Balance = resultFrom.Balance - transfer.Amount,
            };
            resultMov = await _unitOfWork.MovementRepository.Create(movementFrom);
            commit = resultMov.Item1;
            if (!resultMov.Item1)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferError), null);

            var resultAcc = await _unitOfWork.AccountRepository.Update(
                new Account
                {
                    Id = transfer.ToAccountId,
                    Balance = resultTo.Balance + transfer.Amount,
                });
            commit = resultAcc;
            if (!resultAcc)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferError), null);

            resultAcc = await _unitOfWork.AccountRepository.Update(
                new Account
                {
                    Id = transfer.FromAccountId,
                    Balance = resultFrom.Balance - transfer.Amount,
                });
            commit = resultAcc;
            if (!resultAcc)
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.TransferError), null);

            transfer.CreatedAt = (DateTime)movementTo.CreatedAt;
            transfer.Id = (int)movementTo.Id;
            //_ = _unitOfWork.MovementRepository.CreateLog(transfer);

            return (null, null, movementTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EnumHelper.GetEnumDescription(ErrorUsecase.TransferError));
            _unitOfWork.Rollback();
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.TransferError), null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }

    }
}
