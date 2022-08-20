﻿using Dapper;
using Npgsql;
using System.Data;
using System.Data.SqlClient;

namespace SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;

public class SqlDataAccess : ISqlDataAccess
{
    private readonly IConfiguration _config;

    public SqlDataAccess(IConfiguration config)
    {
        _config = config;
    }

    public NpgsqlConnection GetSqlConnection(string connectionId = "Default")
    {
        return new NpgsqlConnection(_config.GetConnectionString(connectionId));
    }

    public async Task<IEnumerable<T>> LoadData<T, U>(
        string storedProcedure,
        U parameters,
        string connectionId = "Default")
    {
        using System.Data.IDbConnection connection = new NpgsqlConnection(_config.GetConnectionString(connectionId));

        return await connection.QueryAsync<T>(storedProcedure, parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> SaveData<T>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default")
    {
        using System.Data.IDbConnection connection = new NpgsqlConnection(_config.GetConnectionString(connectionId));

        return await connection.ExecuteAsync(storedProcedure, parameters,
            commandType: CommandType.StoredProcedure);
    }
    
}

