using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    public IUserRepository UserRepository { get; }
    public IAccountRepository AccountRepository { get; }
    public IMovementRepository MovementRepository { get; }
    //public ITransferRepository transferRepository { get; }
    public ISessionRepository SessionRepository { get; }

    //private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    //private const string _connectionId = "BankDB";
    private readonly IDbTransaction _dbTransaction;
    //private readonly IDbConnection _dbConnection;
    //private readonly ISqlDataAccess _db;
    //Guid _id = Guid.Empty;

    //
    public UnitOfWork(IDbTransaction dbTransaction/*, ISqlDataAccess db*//*, IDbConnection dbConnection*/, IDistributedCache cache, IUserRepository userRepository, IAccountRepository accountRepository, IMovementRepository movementRepository, ISessionRepository sessionRepository)
    {
        UserRepository = userRepository;
        AccountRepository = accountRepository;
        MovementRepository = movementRepository;
        this.SessionRepository = sessionRepository;
        //this.transferRepository = transferRepository;

        _dbTransaction = dbTransaction;
        //_dbConnection = dbConnection;
        _cache = cache;
        //_db = db;
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
        catch(Exception ex)
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