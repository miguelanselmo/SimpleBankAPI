using Microsoft.IdentityModel.Tokens;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SimpleBankAPI.Usecases;

public class TransferUseCase : ITransferUseCase
{
    private readonly ILogger<TransferUseCase> _logger;
    private readonly IUserRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IMovementRepository _movementRepository;

    public TransferUseCase(ILogger<TransferUseCase> logger, IAccountRepository accountRepository, ITransferRepository transferRepository, IMovementRepository movementRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _transferRepository = transferRepository;
        _movementRepository = movementRepository;
    }

    public async Task<(bool,string?, TransferModel?)> Transfer(TransferModel transfer)
    {
        return (false, "", null);
    }    
}
