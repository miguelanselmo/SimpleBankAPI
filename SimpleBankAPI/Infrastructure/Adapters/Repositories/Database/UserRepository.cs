using Dapper;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Database;

internal class UserRepository : IUserRepository
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
        return UserMapper.Map(resultDb);
    }

    public async Task<User?> ReadByName(string name)
    {
        var query = "SELECT * FROM users WHERE username=@username";
        var parameters = new DynamicParameters();
        parameters.Add("username", name);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
        return UserMapper.Map(resultDb);
    }
    public async Task<IEnumerable<User>?> ReadAll()
    {
        var query = "SELECT * FROM users";
        var resultDb = await _dbTransaction.Connection.QueryAsync(query);
        return UserMapper.Map(resultDb);
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
            " email=@Email WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id);
        parameters.Add("username", data.UserName);
        parameters.Add("password", data.Password);
        parameters.Add("full_name", data.FullName);
        parameters.Add("email", data.Email);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    /*
    public async Task<bool> Delete(int id)
    {
        var query = "DELETE FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    */
}
