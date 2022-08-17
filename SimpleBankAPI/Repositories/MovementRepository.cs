using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class MovementRepository : IMovementRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "Movement";

    public MovementRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IEnumerable<MovementModel>> ReadById(int accountId, int id)
    {
        var query = "SELECT * FROM movements WHERE account_id=@Id AND id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
        parameters.Add("Id", id);
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryAsync<object>(query, parameters);
            return Map(resultDb);
        }
    }
    
    public async Task<IEnumerable<MovementModel>> ReadByAccount(int accountId)
    {
        var query = "SELECT * FROM movements WHERE account_id=@account_id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var resultDb = await connection.QueryAsync<object>(query, parameters);
            return Map(resultDb);
        }
    }
    
    public async Task<IEnumerable<MovementModel>> ReadAll()
    {
        var query = "SELECT * FROM movements";
        using (var connection = _db.GetSqlConnection(_connectionId))
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
    
    public async Task<(bool,int?)> Create(MovementModel dataModel)
    {
        var query = "INSERT INTO movements (account_id, amount, balance)"
            + " VALUES(@account_id,  @amount, @balance) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", dataModel.AccountId);
        parameters.Add("amount", dataModel.Amount);
        parameters.Add("balance", dataModel.Balance);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteScalarAsync<int>(query, parameters);
            return result > 0 ? (true, result) : (false, null);
        }
    }

    public async Task<bool> Update(MovementModel dataModel)
    {
        var query = "UPDATE movements SET amount=@amount, balance=@balance" +
            ", WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", dataModel.Id);
        parameters.Add("amount", dataModel.Amount);
        parameters.Add("balance", dataModel.Balance);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM movements WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
        }
    }
}
