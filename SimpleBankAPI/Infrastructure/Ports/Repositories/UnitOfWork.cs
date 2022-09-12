using Microsoft.Extensions.Caching.Distributed;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    public IUserRepository UserRepository { get; }
    public IAccountRepository AccountRepository { get; }
    public IMovementRepository MovementRepository { get; }
    //public ITransferRepository transferRepository { get; }
    public ISessionRepository SessionRepository { get; }

    private readonly IDistributedCache _cache;
    private readonly IDbTransaction _dbTransaction;
    //Guid _id = Guid.Empty;

    //
    public UnitOfWork(IDbTransaction dbTransaction, IDistributedCache cache, IUserRepository userRepository, IAccountRepository accountRepository, IMovementRepository movementRepository, ISessionRepository sessionRepository)
    {
        UserRepository = userRepository;
        AccountRepository = accountRepository;
        MovementRepository = movementRepository;
        SessionRepository = sessionRepository;
        //this.transferRepository = transferRepository;

        _dbTransaction = dbTransaction;
        _cache = cache;
    }

    public void Commit()
    {
        try
        {
            _dbTransaction.Commit();
            //Dispose();
            //_dbTransaction.Connection.BeginTransaction();

        }
        catch (Exception ex)
        {
            _dbTransaction.Rollback();
            throw;
        }
    }

    public IDbTransaction Begin()
    {
        try
        {
            return _dbTransaction.Connection.BeginTransaction();
        }
        catch (Exception ex)
        {
            return _dbTransaction;
        }
    }

    public void Rollback()
    {
        _dbTransaction.Rollback();
        Dispose();
    }

    public void Dispose()
    {
        try
        {
            _dbTransaction.Connection?.Close();
            _dbTransaction.Connection?.Dispose();
            _dbTransaction.Dispose();
        }
        catch (Exception ex)
        {
        }
    }

    ~UnitOfWork()
    {
        Dispose();
    }
}