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

    public async Task<(bool, string?, Movement?)> Transfer(Transfer transfer)
    {
        bool commit = true;
        try
        {
            _unitOfWork.Begin();
            if (transfer.ToAccountId == transfer.FromAccountId)
                return (false, "The accounts are the same.", null);
            var resultFrom = await _unitOfWork.AccountRepository.ReadById(transfer.UserId, transfer.FromAccountId);
            if (resultFrom is null)
                return (false, "Account not found.", null);
            if (transfer.Amount > resultFrom.Balance)
                return (false, "Balance below amount.", null);
            var resultTo = await _unitOfWork.AccountRepository.ReadById(transfer.ToAccountId);
            if (resultTo is null)
                return (false, "Destination account not found.", null);
            if (resultFrom.Currency != resultTo.Currency)
                return (false, "Account with different currencies.", null);

            var resultMov = await _unitOfWork.MovementRepository.Create(
                new Movement
                {
                    AccountId = transfer.ToAccountId,
                    Amount = transfer.Amount,
                    Balance = resultTo.Balance + transfer.Amount,
                });
            commit = resultMov.Item1;
            if (!resultMov.Item1)
                return (false, "Transfer error.", null);

            var movementTo = new Movement
            {
                AccountId = transfer.FromAccountId,
                Amount = transfer.Amount * -1,
                Balance = resultFrom.Balance - transfer.Amount,
            };
            resultMov = await _unitOfWork.MovementRepository.Create(movementTo);
            commit = resultMov.Item1;
            if (!resultMov.Item1)
                return (false, "Transfer error.", null);

            var resultAcc = await _unitOfWork.AccountRepository.Update(
                new Account
                {
                    Id = transfer.ToAccountId,
                    Balance = resultTo.Balance + transfer.Amount,
                });
            commit = resultAcc;
            if (!resultAcc)
                return (false, "Transfer error.", null);

            resultAcc = await _unitOfWork.AccountRepository.Update(
                new Account
                {
                    Id = transfer.FromAccountId,
                    Balance = resultFrom.Balance - transfer.Amount,
                });
            commit = resultAcc;
            if (!resultAcc)
                return (false, "Transfer error.", null);

            transfer.CreatedAt = movementTo.CreatedAt;
            transfer.Id = movementTo.Id;
            //_ = _unitOfWork.MovementRepository.CreateLog(transfer);

            return (true, null, movementTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on transfer.");
            _unitOfWork.Rollback();
            return (false, "Error on transfer.", null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }

    }
}
