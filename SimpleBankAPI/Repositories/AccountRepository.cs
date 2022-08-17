using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "Account";

    public AccountRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<AccountModel?> ReadById(int userId, int id)
    {
        var query = "SELECT * FROM accounts WHERE id=@id AND user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("user_id", userId);
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            return Map(resultDb);
        }
    }

    public async Task<AccountModel?> ReadById(int id)
    {
        var query = "SELECT * FROM accounts WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
                using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            return Map(resultDb);
        }
    }

    public async Task<IEnumerable<AccountModel?>> ReadByUser(int userId)
    {
        var query = "SELECT * FROM accounts WHERE user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", userId);
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryAsync<object>(query, parameters);
            return Map(resultDb);
        }
    }
    
    public async Task<IEnumerable<AccountModel>> ReadAll()
    {
        var query = "SELECT * FROM accounts";
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryAsync(query);
            return Map(resultDb);
        }
    }    
    
    private static IEnumerable<AccountModel> Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        return dataDb.Select(x => new AccountModel
        {
            Id = (int)x.id,
            UserId = (int)x.user_id,
            Balance = (decimal)x.balance,
            Currency = Enum.Parse<CurrencyEnum>(x.currency),
            CreatedAt = (DateTime)x.created_at
        });
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
    
    public async Task<(bool,int?)> Create(AccountModel dataModel)
    {
        var query = "INSERT INTO accounts (user_id, balance, currency)"
            + " VALUES(@user_id,  @balance,  @currency) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", dataModel.UserId);
        parameters.Add("balance", dataModel.Balance);
        parameters.Add("currency", dataModel.Currency);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteScalarAsync<int>(query, parameters);
            return result > 0 ? (true, result) : (false, null);
        }
    }
    
    public async Task<bool> Update(AccountModel dataModel)
    {
        var query = "UPDATE accounts SET balance=@balance" +
            ", WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", dataModel.Id);
        parameters.Add("balance", dataModel.Balance);
        
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM accounts WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
        }
    }
    
}
