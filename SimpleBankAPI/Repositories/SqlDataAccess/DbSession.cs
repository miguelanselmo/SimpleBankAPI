using Npgsql;
using System.Data;

namespace SimpleBankAPI.Repositories.SqlDataAccess;

public sealed class DbSession : IDisposable
{
    private readonly IConfiguration _config;
    
    private Guid _id;
    public IDbConnection Connection { get; }
    public IDbTransaction Transaction { get; set; }

    public DbSession(IConfiguration config, string connectionId = "Default")
    {
        _config = config;
        _id = Guid.NewGuid();
        Connection = new NpgsqlConnection(_config.GetConnectionString(connectionId));
        Connection.Open();
    }

    public void Dispose() => Connection?.Dispose();
}