

using Npgsql;

namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface ISqlDataAccess
{

    NpgsqlConnection GetSqlConnection(string connectionId = "Default");
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionId = "Default");
    Task<int> SaveData<T>(string storedProcedure, T parameters, string connectionId = "Default");
}