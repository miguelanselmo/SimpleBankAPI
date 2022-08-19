using Npgsql;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;

public interface IDbSession : IDisposable
{
    public void Dispose();
}