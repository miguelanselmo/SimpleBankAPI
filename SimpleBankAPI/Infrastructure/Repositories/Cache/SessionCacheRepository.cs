using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories.Mapper;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

internal class SessionCacheRepository : ISessionRepository
{
    private readonly IDbTransaction _dbTransaction;
    private readonly IDistributedCache _cache;
    private const string _caheKey = "Session";

    public SessionCacheRepository(IDbTransaction dbTransaction, IDistributedCache cache)
    {
        _dbTransaction = dbTransaction;
        _cache = cache;
    }

    public async Task<Session?> ReadById(Guid id)
    {
        var resultCache = await _cache.GetRecordAsync<Session>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var query = "SELECT * FROM sessions WHERE id=@id";
            var parameters = new DynamicParameters();
            parameters.Add("id", id.ToString());
            var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            return SessionMapper.Map(resultDb);
        }
        else
            return resultCache;
    }

    public async Task<IEnumerable<Session>?> ReadByUser(int userId)
    {
        var resultCache = await _cache.GetRecordAsync<Session[]>(_caheKey + ":" + "*");
        if (resultCache is null)
        {
            var query = "SELECT * FROM sessions WHERE user_id=@user_id";
            var parameters = new DynamicParameters();
            parameters.Add("user_id", userId);
            var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters);
            return SessionMapper.Map(resultDb);
        }
        else
            return resultCache.Where(x => x.UserId.Equals(userId));
    }
    /*
    public async Task<IEnumerable<Session>?> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<Session[]>(_caheKey + "*");
        if (resultCache is null)
        {
            var query = "SELECT * FROM sessions";
            var resultDb = await _dbTransaction.Connection.QueryAsync(query);
            return Map(resultDb);
        }
        else
            return resultCache;
    }
    */
    public async Task<bool> Create(Session data)
    {
        var query = "INSERT INTO sessions (id, user_id)"
            + " VALUES(@id, @user_id)";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id.ToString());
        parameters.Add("user_id", data.UserId);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        if (result > 0) { await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data); }
        return result > 0;
    }

    public async Task<bool> Update(Session data)
    {
        var query = "UPDATE sessions SET active=@active WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id.ToString());
        parameters.Add("active", data.Active);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        await _cache.RemoveAsync(_caheKey + data.Id);
        await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);
        return result > 0;
    }
    /*
    public async Task<bool> Delete(Guid id)
    {
        var query = "DELETE FROM sessions WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id.ToString());
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        await _cache.RemoveAsync(_caheKey + id);
        return result > 0;
    }
    */
}
