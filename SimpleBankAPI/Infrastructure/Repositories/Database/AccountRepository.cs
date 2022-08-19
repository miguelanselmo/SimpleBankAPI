using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Core.Enums;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    //private readonly ISqlDataAccess db;
    private readonly IDbTransaction dbTransaction;
    private const string connectionId = "BankDB";
    
    public AccountRepository(/*ISqlDataAccess db,*/ IDbTransaction dbTransaction)
    {
        //this.db = db;
        this.dbTransaction = dbTransaction;
    }

    public async Task<AccountModel?> ReadById(int userId, int id)
    {
        var query = "SELECT * FROM accounts WHERE id=@id AND user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("user_id", userId);
            var resultDb = await dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }

    public async Task<AccountModel?> ReadById(int id)
    {
        var query = "SELECT * FROM accounts WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
            var resultDb = await dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }

    public async Task<IEnumerable<AccountModel?>> ReadByUser(int userId)
    {
        var query = "SELECT * FROM accounts WHERE user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", userId);
            var resultDb = await dbTransaction.Connection.QueryAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }

    public async Task<IEnumerable<AccountModel>> ReadAll()
    {
        var query = "SELECT * FROM accounts";
            var resultDb = await dbTransaction.Connection.QueryAsync(query);
            return Map(resultDb);
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

    public async Task<(bool, int?)> Create(AccountModel dataModel)
    {
        var query = "INSERT INTO accounts (user_id, balance, currency)"
            + " VALUES(@user_id,  @balance,  @currency) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", dataModel.UserId);
        parameters.Add("balance", dataModel.Balance);
        parameters.Add("currency", dataModel.Currency);

            var result = await dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, dbTransaction);
            return result > 0 ? (true, result) : (false, null);
    }

    public async Task<bool> Update(AccountModel dataModel)
    {
        var query = "UPDATE accounts SET balance=@balance WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", dataModel.Id);
        parameters.Add("balance", dataModel.Balance);

            var result = await dbTransaction.Connection.ExecuteAsync(query, parameters, dbTransaction);
            return result > 0;
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM accounts WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

            var result = await dbTransaction.Connection.ExecuteAsync(query, parameters, dbTransaction);
            return result > 0;
    }

}
