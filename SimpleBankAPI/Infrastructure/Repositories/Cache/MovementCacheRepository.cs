using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Core.Enums;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;
using System.Text.Json;

namespace SimpleBankAPI.Infrastructure.Repositories;

internal class MovementCacheRepository : IMovementRepository
{
    private readonly IDbTransaction _dbTransaction;
    private readonly IDistributedCache _cache;
    private const string _caheKey = "Movement";

    public MovementCacheRepository(IDbTransaction dbTransaction, IDistributedCache cache)
    {
        _dbTransaction = dbTransaction;
        _cache = cache;
    }

    public async Task<Movement?> ReadById(int accountId, int id)
    {
        var resultCache = await _cache.GetRecordAsync<Movement>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var query = "SELECT * FROM movements WHERE account_id=@Id AND id=@id";
            var parameters = new DynamicParameters();
            parameters.Add("account_id", accountId);
            parameters.Add("Id", id);
            var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            return Map(resultDb);
        }
        else
            return resultCache;
    }

    public async Task<IEnumerable<Movement>?> ReadByAccount(int accountId)
    {
        var resultCache = await _cache.GetRecordAsync<Movement[]>(_caheKey + ":" + "*");
        if (resultCache is null)
        {
            var query = "SELECT * FROM movements WHERE account_id=@account_id";
            var parameters = new DynamicParameters();
            parameters.Add("account_id", accountId);
            var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters);
            return Map(resultDb);
        }
        else
            return resultCache.Where(x => x.AccountId.Equals(accountId));
    }

    /*
    public async Task<IEnumerable<Movement>?> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<Movement[]>(_caheKey);
        if (resultCache is null)
        {
            var query = "SELECT * FROM movements";
            var resultDb = await _dbTransaction.Connection.QueryAsync(query);
            return Map(resultDb);
        }
        else
            return resultCache;
    }
    */
    private static IEnumerable<Movement>? Map(IEnumerable<dynamic> dataDb)
    {
        IEnumerable<Movement> MovementList = dataDb.Select(x => new Movement
        {
            Id = (int)x.id,
            AccountId = (int)x.account_id,
            Amount = (decimal)x.amount,
            Balance = (decimal)x.balance,
            CreatedAt = (DateTime)x.created_at,
        });
        return MovementList;
    }

    private static Movement? Map(dynamic x)
    {
        return new Movement
        {
            Id = (int)x.id,
            AccountId = (int)x.account_id,
            Amount = (decimal)x.amount,
            Balance = (decimal)x.balance,
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(Movement data)
    {
        var query = "INSERT INTO movements (account_id, amount, balance)"
            + " VALUES(@account_id,  @amount, @balance) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", data.AccountId);
        parameters.Add("amount", data.Amount);
        parameters.Add("balance", data.Balance);
        var result = await _dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, _dbTransaction);
        if (result > 0) { data.Id = result; await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data); }
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
        await _cache.RemoveAsync(_caheKey + data.Id);
        await _cache.SetRecordAsync(_caheKey + data.Id, data);
        return result > 0;
    }

    public async Task<bool> Delete(int id)
    {
        var query = "DELETE FROM movements WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        await _cache.RemoveAsync(_caheKey + id);
        return result > 0;
    }
    */
}
