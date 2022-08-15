namespace SimpleBankAPI.Repositories.SqlDataAccess;

public interface IUnitOfWork : IDisposable
{
    void BeginTransaction();
    void Commit();
    void Rollback();
}