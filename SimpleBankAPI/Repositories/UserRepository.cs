﻿using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "User";

    public UserRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<UserModel?> ReadById(int id)
    {
        var query = "SELECT * FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            return Map(resultDb);
        }
    }
    
    public async Task<IEnumerable<UserModel>> ReadAll()
    {
        var query = "SELECT * FROM users";
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryAsync(query);
            return Map(resultDb);
        }
    }

    private static IEnumerable<UserModel> Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<UserModel> userList = dataDb.Select(x => new UserModel
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
        });
        return userList;
    }

    private static UserModel Map(dynamic x)
    {
        if (x is null) return null;
        return new UserModel
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
        };
    }
    
    public async Task<(bool,int?)> Create(UserModel dataModel)
    {
        var query = "INSERT INTO users (username, password, full_name, email)"
            + " VALUES(@username,  @password,  @full_name, @email) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("username", dataModel.UserName);
        parameters.Add("password", dataModel.Password);
        parameters.Add("full_name", dataModel.FullName);
        parameters.Add("email", dataModel.Email);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteScalarAsync<int>(query, parameters);
            return result > 0 ? (true,result) : (false,null);
        }
    }

    public async Task<bool> Update(UserModel dataModel)
    {
        var query = "UPDATE users SET username=@username, password=@password, full_name=@full_name" +
            ", email=@Email WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", dataModel.Id);
        parameters.Add("username", dataModel.UserName);
        parameters.Add("password", dataModel.Password);
        parameters.Add("full_name", dataModel.FullName);
        parameters.Add("email", dataModel.Email);
        
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
        }
    }
}
