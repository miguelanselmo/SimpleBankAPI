using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SimpleBankAPI.Core.Usecases;

public class TransferUseCase : ITransferUseCase
{
    private readonly ILogger<TransferUseCase> logger;
    private readonly IUnitOfWork unitOfWork;

    public TransferUseCase(ILogger<TransferUseCase> logger, IUnitOfWork unitOfWork)
    {
        this.logger = logger;
        this.unitOfWork = unitOfWork;
    }

    public async Task<(bool, string?)> Transfer(TransferModel transfer)
    {
        //TODO: rollback em caso de erro        
        bool commit = true;
        try
        {
            unitOfWork.Begin();
            if (transfer.ToAccountId == transfer.FromAccountId)
            return (false, "The accounts are the same.");
            var resultFrom = await unitOfWork.AccountRepository.ReadById(transfer.UserId, transfer.FromAccountId);
            if (resultFrom is null)
                return (false, "Account not found.");
            if (transfer.Amount > resultFrom.Balance)
                return (false, "Balance below amount.");
            var resultTo = await unitOfWork.AccountRepository.ReadById(transfer.ToAccountId);
            if (resultTo is null)
                return (false, "Destination account not found.");
            if (resultFrom.Currency != resultTo.Currency)
                return (false, "Account with different currencies.");       
            
            var resultMov = await unitOfWork.MovementRepository.Create(
                new MovementModel
                {
                    AccountId = transfer.ToAccountId,
                    Amount = transfer.Amount,
                    Balance = resultTo.Balance + transfer.Amount,
                });
            commit = resultMov.Item1;
            if (!resultMov.Item1)
                return (false, "Transfer error.");
            resultMov = await unitOfWork.MovementRepository.Create(
                new MovementModel
                {
                    AccountId = transfer.FromAccountId,
                    Amount = transfer.Amount * -1,
                    Balance = resultFrom.Balance - transfer.Amount,
                });
            commit = resultMov.Item1;
            if (!resultMov.Item1)
                return (false, "Transfer error.");

            var resultAcc = await unitOfWork.AccountRepository.Update(
                new AccountModel
                {
                    Id = transfer.ToAccountId,
                    Balance = resultTo.Balance + transfer.Amount,
                });
            commit = resultAcc;
            if (!resultAcc)
                return (false, "Transfer error.");

            resultAcc = await unitOfWork.AccountRepository.Update(
            new AccountModel
            {
                Id = transfer.FromAccountId,
                Balance = resultFrom.Balance - transfer.Amount,
            });
            commit = resultAcc;
            if (!resultAcc)
                return (false, "Transfer error.");

            //TODO: insert operation log
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on transfer.");
            unitOfWork.Rollback();
            return (false, "Error on transfer.");
        }
        finally
        {
            if (commit) unitOfWork.Commit(); else unitOfWork.Rollback();
        }
        return (true, null);
    }
}
