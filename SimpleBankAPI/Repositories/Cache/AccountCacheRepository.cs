using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories.Cache;

public class AccountCacheRepository : IAccountRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "Account";

    public AccountCacheRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<AccountModel?> ReadById(int userId, int id)
    {
        var resultCache = await _cache.GetRecordAsync<AccountModel[]>(_caheKey + id);
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
                AccountModel dataModel = Map(resultDb);
                //await _cache.SetRecordAsync(_caheKey+id, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<AccountModel?>> ReadByUserId(int userId)
    {
        var resultCache = await _cache.GetRecordAsync<AccountModel[]>(_caheKey + userId);
        if (resultCache is null)
        {
            var query = "SELECT * FROM accounts WHERE user_id=@user_id";
            var parameters = new DynamicParameters();
            parameters.Add("user_id", userId);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync<object>(query, parameters);
                IEnumerable<AccountModel> dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey + userId, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Where(x => x.UserId.Equals(userId));
    }

    public async Task<IEnumerable<AccountModel>> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<AccountModel[]>(_caheKey);
        if (resultCache is null)
        {
            var query = "SELECT * FROM accounts";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                IEnumerable<AccountModel> dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey, dataModel);
                return dataModel;
            }
        }
        else
            return Map(resultCache);
    }

    private static IEnumerable<AccountModel> Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<AccountModel> AccountList = dataDb.Select(x => new AccountModel
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = Enum.Parse<CurrencyEnum>(x.currency),
            CreatedAt = (DateTime)x.created_at
        });
        return AccountList;
    }

    private static AccountModel Map(dynamic x)
    {
        if (x is null) return null;
        return new AccountModel
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = Enum.Parse<CurrencyEnum>(x.currency),
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(AccountModel dataModel)
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

    public async Task<bool> Update(AccountModel dataModel)
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

    public Task<AccountModel?> ReadById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AccountModel?>> ReadByUser(int userId)
    {
        throw new NotImplementedException();
    }
}
