using System.Data;

namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IAccountRepository AccountRepository { get; }
    IMovementRepository MovementRepository { get; }
    ISessionRepository SessionRepository { get; }
    IDocumentRepository DocumentRepository { get; }
    //ITransferRepository transferRepository { get; }
    //Guid Id { get; }
    //IDbConnection Connection { get; }
    //IDbTransaction Transaction { get; }
    IDbTransaction Begin();
    void Commit();
    void Rollback();
}