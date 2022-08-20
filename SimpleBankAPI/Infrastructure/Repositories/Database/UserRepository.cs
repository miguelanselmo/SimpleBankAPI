using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbTransaction _dbTransaction;

    public UserRepository(IDbTransaction dbTransaction)
    {
        _dbTransaction = dbTransaction;
    }

    public async Task<User?> ReadById(int id)
    {
        var query = "SELECT * FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
        return Map(resultDb);
    }

    public async Task<User?> ReadByName(string name)
    {
        var query = "SELECT * FROM users WHERE username=@username";
        var parameters = new DynamicParameters();
        parameters.Add("username", name);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
        return Map(resultDb);
    }
    public async Task<IEnumerable<User>> ReadAll()
    {
        var query = "SELECT * FROM users";
        var resultDb = await _dbTransaction.Connection.QueryAsync(query);
        return Map(resultDb);
    }

    private static IEnumerable<User> Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<User> userList = dataDb.Select(x => new User
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

    private static User Map(dynamic x)
    {
        if (x is null) return null;
        return new User
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(User data)
    {
        var query = "INSERT INTO users (username, password, full_name, email)"
            + " VALUES(@username,  @password,  @full_name, @email) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("username", data.UserName);
        parameters.Add("password", data.Password);
        parameters.Add("full_name", data.FullName);
        parameters.Add("email", data.Email);
        var result = await _dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, _dbTransaction);
        return result > 0 ? (true, result) : (false, null);
    }

    public async Task<bool> Update(User data)
    {
        var query = "UPDATE users SET username=@username, password=@password, full_name=@full_name" +
            ", email=@Email WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id);
        parameters.Add("username", data.UserName);
        parameters.Add("password", data.Password);
        parameters.Add("full_name", data.FullName);
        parameters.Add("email", data.Email);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
}
