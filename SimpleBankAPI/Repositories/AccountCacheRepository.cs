using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories.Cache;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

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

    public async Task<AccountModel?> Read(int id)
    {
        var resultCache = _cache.GetRecordAsync<AccountModel[]>(_caheKey+id);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM accounts WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                AccountModel Account = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey+id, Account);
                return Account;
            }
        }
        else
            return resultCache.Result.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<AccountModel>> Read()
    {
        var resultCache = _cache.GetRecordAsync<AccountModel[]>(_caheKey);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM accounts";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                return Map(resultDb);
            }
        }
        else
            return Map(resultCache.Result);
    }

    private static IEnumerable<AccountModel> Map(IEnumerable<dynamic> AccountDb)
    {
    IEnumerable<AccountModel> AccountList = AccountDb.Select(x => new AccountModel
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = (CurrencyEnum)x.currency,
            CreatedAt = (DateTime)x.created_at,
        });
        return AccountList;
    }

    private static AccountModel Map(dynamic x)
    {
        return new AccountModel
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = (CurrencyEnum)x.currency,
            CreatedAt = (DateTime)x.created_at,
        };
    }
    
    public async Task<bool> Create(AccountModel Account)
    {
        var query = "INSERT INTO accounts (user_id, balance, currency)"
            + " VALUES(@UserId,  @Balance,  @Currency)";
        var parameters = new DynamicParameters();
        parameters.Add("UserId", Account.UserId);
        parameters.Add("Balance", Account.Balance);
        parameters.Add("Currency", Account.Currency);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.SetRecordAsync(_caheKey + Account.Id, Account);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Update(AccountModel Account)
    {
        var query = "UPDATE accounts SET balance=@Balance" +
            ", WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", Account.Id);
        parameters.Add("Balance", Account.Balance);
        
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey+ Account.Id);
                await _cache.SetRecordAsync(_caheKey + Account.Id, Account);
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
                await _cache.RemoveAsync(_caheKey + id);
                return true;
            }
            return false;
        }
    }
}
