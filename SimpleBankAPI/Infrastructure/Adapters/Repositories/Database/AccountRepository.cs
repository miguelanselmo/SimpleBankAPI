using Dapper;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Database;

internal class AccountRepository : IAccountRepository
{
    private readonly IDbTransaction _dbTransaction;

    public AccountRepository(IDbTransaction dbTransaction)
    {
        _dbTransaction = dbTransaction;
    }

    public async Task<Account?> ReadById(int userId, int id)
    {
        var query = "SELECT * FROM accounts WHERE id=@id AND user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("user_id", userId);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, _dbTransaction);
        return AccountMapper.Map(resultDb);
    }

    public async Task<Account?> ReadById(int id)
    {
        var query = "SELECT * FROM accounts WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, _dbTransaction);
        return AccountMapper.Map(resultDb);
    }

    public async Task<IEnumerable<Account>?> ReadByUser(int userId)
    {
        var query = "SELECT * FROM accounts WHERE user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", userId);
        var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters, _dbTransaction);
        return AccountMapper.Map(resultDb);
    }
    /*
    public async Task<IEnumerable<Account>?> ReadAll()
    {
        var query = "SELECT * FROM accounts";
        var resultDb = await _dbTransaction.Connection.QueryAsync(query);
        return Map(resultDb);
    }
    */

    public async Task<(bool, int?)> Create(Account data)
    {
        var query = "INSERT INTO accounts (user_id, balance, currency)"
            + " VALUES(@user_id,  @balance,  @currency) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", data.UserId);
        parameters.Add("balance", data.Balance);
        parameters.Add("currency", data.Currency);
        var result = await _dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, _dbTransaction);
        return result > 0 ? (true, result) : (false, null);
    }

    public async Task<bool> Update(Account data)
    {
        var query = "UPDATE accounts SET balance=@balance WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id);
        parameters.Add("balance", data.Balance);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    /*
    public async Task<bool> Delete(int id)
    {
        var query = "DELETE FROM accounts WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    */
}
