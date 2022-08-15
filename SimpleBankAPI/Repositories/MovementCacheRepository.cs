using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories.Cache;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class MovementCacheRepository : IMovementRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "Movement";

    public MovementCacheRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<MovementModel?> Read(int id)
    {
        var resultCache = _cache.GetRecordAsync<MovementModel[]>(_caheKey+id);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM movements WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                MovementModel dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey+id, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Result.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<MovementModel>> ReadByAccount(int id)
    {
        var resultCache = _cache.GetRecordAsync<MovementModel[]>(_caheKey + id);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM movements WHERE account_id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                IEnumerable<MovementModel> dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey+id, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Result.Where(x => x.Id.Equals(id));
    }
    
    public async Task<IEnumerable<MovementModel>> Read()
    {
        var resultCache = _cache.GetRecordAsync<MovementModel[]>(_caheKey);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM Movements";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                return Map(resultDb);
            }
        }
        else
            return Map(resultCache.Result);
    }

    private static IEnumerable<MovementModel> Map(IEnumerable<dynamic> dataDb)
    {
        IEnumerable<MovementModel> MovementList = dataDb.Select(x => new MovementModel
        {
            Id = (int)x.id,
            AccountId = (int)x.account_id,
            Amount = (decimal)x.amount,
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
            CreatedAt = (DateTime)x.created_at,
        };
    }
    
    public async Task<bool> Create(MovementModel dataModel)
    {
        var query = "INSERT INTO movements (account_id, amount)"
            + " VALUES(@AccountId,  @Amount)";
        var parameters = new DynamicParameters();
        parameters.Add("FromAccountId", dataModel.AccountId);
        parameters.Add("Amount", dataModel.Amount);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.SetRecordAsync(_caheKey + dataModel.Id, dataModel);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Update(MovementModel dataModel)
    {
        var query = "UPDATE movements SET amount=@Amount" +
            ", WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", dataModel.Id);
        parameters.Add("Amount", dataModel.Amount);
        
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey+ dataModel.Id);
                await _cache.SetRecordAsync(_caheKey + dataModel.Id, dataModel);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM movements WHERE id=@Id";
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
