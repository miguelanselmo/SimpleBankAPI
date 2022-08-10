﻿using System.Data.SqlClient;

namespace SimpleBankAPI.Repositories.SqlDataAccess;

public interface ISqlDataAccess
{

    SqlConnection GetSqlConnection(string connectionId = "Default");
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionId = "Default");
    Task<int> SaveData<T>(string storedProcedure, T parameters, string connectionId = "Default");
}