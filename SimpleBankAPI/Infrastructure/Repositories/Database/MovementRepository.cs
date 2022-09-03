using Dapper;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories.Mapper;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories.Database;

internal class MovementRepository : IMovementRepository
{
    private readonly IDbTransaction _dbTransaction;

    public MovementRepository(IDbTransaction dbTransaction)
    {
        _dbTransaction = dbTransaction;
    }

    public async Task<Movement?> ReadById(int accountId, int id)
    {
        var query = "SELECT * FROM movements WHERE account_id=@Id AND id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
        parameters.Add("Id", id);
        var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters);
        return MovementMapper.Map(resultDb).FirstOrDefault();
    }

    public async Task<IEnumerable<Movement>?> ReadByAccount(int accountId)
    {
        var query = "SELECT * FROM movements WHERE account_id=@account_id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
        var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters);
        return MovementMapper.Map(resultDb);
    }
    /*
    public async Task<IEnumerable<Movement>?> ReadAll()
    {
        var query = "SELECT * FROM movements";
        var resultDb = await _dbTransaction.Connection.QueryAsync(query);
        return Map(resultDb);
    }
    */

    public async Task<(bool, int?)> Create(Movement data)
    {
        var query = "INSERT INTO movements (account_id, amount, balance)"
            + " VALUES(@account_id,  @amount, @balance) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", data.AccountId);
        parameters.Add("amount", data.Amount);
        parameters.Add("balance", data.Balance);
        var result = await _dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, _dbTransaction);
        return result > 0 ? (true, result) : (false, null);
    }

    public async Task<(bool, int?)> CreateLog(Transfer data)
    {
        var query = "INSERT INTO operations_log (data)"
            + " VALUES(@data) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("data", null);
        var result = await _dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, _dbTransaction);
        return result > 0 ? (true, result) : (false, null);
    }
    /*
    public async Task<bool> Update(Movement data)
    {
        var query = "UPDATE movements SET amount=@amount, balance=@balance WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id);
        parameters.Add("amount", data.Amount);
        parameters.Add("balance", data.Balance);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }

    public async Task<bool> Delete(int id)
    {
        var query = "DELETE FROM movements WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    */
}
