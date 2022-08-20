using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Core.Enums;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

public class AccountCacheRepository : IAccountRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "Account";

    public AccountCacheRepository(ISqlDataAccess db, IDbTransaction dbTransaction, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Account?> ReadById(int userId, int id)
    {
        var resultCache = await _cache.GetRecordAsync<Account[]>(_caheKey + id);
        if (resultCache is null)
        {
            var query = "SELECT * FROM accounts WHERE id=@id AND user_id=@user_id";
            var parameters = new DynamicParameters();
            parameters.Add("id", id);
            parameters.Add("user_id", userId);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                Account dataModel = Map(resultDb);
                //await _cache.SetRecordAsync(_caheKey+id, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<Account?>> ReadByUserId(int userId)
    {
        var resultCache = await _cache.GetRecordAsync<Account[]>(_caheKey + userId);
        if (resultCache is null)
        {
            var query = "SELECT * FROM accounts WHERE user_id=@user_id";
            var parameters = new DynamicParameters();
            parameters.Add("user_id", userId);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync<object>(query, parameters);
                IEnumerable<Account> dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey + userId, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Where(x => x.UserId.Equals(userId));
    }

    public async Task<IEnumerable<Account>> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<Account[]>(_caheKey);
        if (resultCache is null)
        {
            var query = "SELECT * FROM accounts";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                IEnumerable<Account> dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey, dataModel);
                return dataModel;
            }
        }
        else
            return Map(resultCache);
    }

    private static IEnumerable<Account> Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<Account> AccountList = dataDb.Select(x => new Account
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = Enum.Parse<Currency>(x.currency),
            CreatedAt = (DateTime)x.created_at
        });
        return AccountList;
    }

    private static Account Map(dynamic x)
    {
        if (x is null) return null;
        return new Account
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = Enum.Parse<Currency>(x.currency),
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(Account dataModel)
    {
        var query = "INSERT INTO accounts (user_id, balance, currency)"
            + " VALUES(@UserId,  @Balance,  @Currency) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("UserId", dataModel.UserId);
        parameters.Add("Balance", dataModel.Balance);
        parameters.Add("Currency", dataModel.Currency);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteScalarAsync<int>(query, parameters);
            if (result > 0)
            {
                await _cache.SetRecordAsync(_caheKey + dataModel.Id, dataModel);
                //await _cache.RemoveAsync(_caheKey);
                return (true, result);
            }
            return (false, null);
        }
    }

    public async Task<bool> Update(Account dataModel)
    {
        var query = "UPDATE accounts SET balance=@Balance" +
            ", WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", dataModel.Id);
        parameters.Add("Balance", dataModel.Balance);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey);
                //await _cache.SetRecordAsync(_caheKey + dataModel.Id, dataModel);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM accounts WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey);
                return true;
            }
            return false;
        }
    }

    public Task<Account?> ReadById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Account?>> ReadByUser(int userId)
    {
        throw new NotImplementedException();
    }
}
