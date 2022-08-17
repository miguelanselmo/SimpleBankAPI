using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SimpleBankAPI.Usecases;

public class TransferUseCase : ITransferUseCase
{
    private readonly ILogger<TransferUseCase> logger;
    private readonly IUserRepository repository;
    private readonly IAccountRepository accountRepository;
    private readonly IMovementRepository movementRepository;

    public TransferUseCase(ILogger<TransferUseCase> logger, IAccountRepository accountRepository, IMovementRepository movementRepository)
    {
        this.logger = logger;
        this.accountRepository = accountRepository;
        this.movementRepository = movementRepository;
    }

    public async Task<(bool, string?)> Transfer(TransferModel transfer)
    {
        var result = await accountRepository.ReadById(transfer.UserId, transfer.FromAccountId);
        if (result is null)
            return (false, "Account not found.");
        if (transfer.Amount > result.Balance)
            return (false, "Balance below amount.");
        var resultTo = await accountRepository.ReadById(transfer.ToAccountId);
        if (resultTo is null)
            return (false, "Destination account not found.");
        if (result.Currency != resultTo.Currency)
            return (false, "Account with different currencies.");
        var resultMov = await movementRepository.Create(
            new MovementModel
            {
                AccountId = transfer.ToAccountId,
                Amount = transfer.Amount * -1,
                Balance = result.Balance - transfer.Amount,
            });
        if (!resultMov.Item1)
            return (false, "Transfer error.");
        resultMov = await movementRepository.Create(
            new MovementModel
            {
                AccountId = transfer.FromAccountId,
                Amount = transfer.Amount,
                Balance = result.Balance + transfer.Amount,
            });
        if (!resultMov.Item1)
            return (false, "Transfer error.");

        var resultAcc = await accountRepository.Update(
            new AccountModel
            {
                Id = transfer.FromAccountId,
                Balance = result.Balance - transfer.Amount,
            });
        if (!resultAcc)
            return (false, "Transfer error.");

        resultAcc = await accountRepository.Update(
            new AccountModel
            {
                Id = transfer.ToAccountId,
                Balance = result.Balance + transfer.Amount,
            });
        if (!resultAcc)
            return (false, "Transfer error.");
        return (true, null);
    }
}
