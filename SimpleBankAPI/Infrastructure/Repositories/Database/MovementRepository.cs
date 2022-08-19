using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Core.Enums;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

public class MovementRepository : IMovementRepository
{
    //private readonly ISqlDataAccess db;
    private readonly IDbTransaction dbTransaction;
    //private const string connectionId = "BankDB";

    public MovementRepository(/*ISqlDataAccess db,*/ IDbTransaction dbTransaction)
    {
        //this.db = db;
        this.dbTransaction = dbTransaction;
    }

    public async Task<IEnumerable<MovementModel>> ReadById(int accountId, int id)
    {
        var query = "SELECT * FROM movements WHERE account_id=@Id AND id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
        parameters.Add("Id", id);
        
            var resultDb = await dbTransaction.Connection.QueryAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }

    public async Task<IEnumerable<MovementModel>> ReadByAccount(int accountId)
    {
        var query = "SELECT * FROM movements WHERE account_id=@account_id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
            var resultDb = await dbTransaction.Connection.QueryAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }

    public async Task<IEnumerable<MovementModel>> ReadAll()
    {
        var query = "SELECT * FROM movements";
        using (var connection = dbTransaction.Connection)
        {
            var resultDb = await connection.QueryAsync(query);
            return Map(resultDb);
        }
    }

    private static IEnumerable<MovementModel> Map(IEnumerable<dynamic> dataDb)
    {
        IEnumerable<MovementModel> MovementList = dataDb.Select(x => new MovementModel
        {
            Id = (int)x.id,
            AccountId = (int)x.account_id,
            Amount = (decimal)x.amount,
            Balance = (decimal)x.balance,
            CreatedAt = (DateTime)x.created_at,
        });
        return MovementList;
    }

    private static MovementModel Map(dynamic x)
    {
        return new MovementModel
        {
            Id = (int)x.id,
            AccountId = (int)x.account_id,
            Amount = (decimal)x.amount,
            Balance = (decimal)x.balance,
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(MovementModel dataModel)
    {
        var query = "INSERT INTO movements (account_id, amount, balance)"
            + " VALUES(@account_id,  @amount, @balance) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", dataModel.AccountId);
        parameters.Add("amount", dataModel.Amount);
        parameters.Add("balance", dataModel.Balance);

            var result = await dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, dbTransaction);
            return result > 0 ? (true, result) : (false, null);
    }

    public async Task<bool> Update(MovementModel dataModel)
    {
        var query = "UPDATE movements SET amount=@amount, balance=@balance WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", dataModel.Id);
        parameters.Add("amount", dataModel.Amount);
        parameters.Add("balance", dataModel.Balance);

            var result = await dbTransaction.Connection.ExecuteAsync(query, parameters, dbTransaction);
            return result > 0;
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM movements WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

            var result = await dbTransaction.Connection.ExecuteAsync(query, parameters, dbTransaction);
            return result > 0;
    }
}
