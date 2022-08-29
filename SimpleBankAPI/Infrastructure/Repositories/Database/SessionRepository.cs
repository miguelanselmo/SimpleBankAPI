using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

internal class SessionRepository : ISessionRepository
{
    private readonly IDbTransaction _dbTransaction;

    public SessionRepository(IDbTransaction dbTransaction)
    {
        _dbTransaction = dbTransaction;
    }

    public async Task<Session?> ReadById(Guid id)
    {
        var query = "SELECT * FROM sessions WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id.ToString());
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
        return Map(resultDb);
    }

    public async Task<IEnumerable<Session>?> ReadByUser(int userId)
    {
        var query = "SELECT * FROM sessions WHERE user_id=@user_id";
        var parameters = new DynamicParameters();
        parameters.Add("user_id", userId);
        var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters);
        return Map(resultDb);
    }
    /*
    public async Task<IEnumerable<Session>?> ReadAll()
    {
        var query = "SELECT * FROM sessions";
        var resultDb = await _dbTransaction.Connection.QueryAsync(query);
        return Map(resultDb);
    }
    */
    private static IEnumerable<Session>? Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<Session> sessionList = dataDb.Select(x => new Session
        {
            Id = Guid.Parse(x.id),
            UserId = (int)x.user_id,
            Active = (bool)x.active,
            CreatedAt = (DateTime)x.created_at,
            TokenRefresh = (string)x.refresk_token,
            TokenRefreshExpireAt = (DateTime)x.refresk_token_expire_at,
        });
        return sessionList;
    }

    private static Session? Map(dynamic x)
    {
        if (x is null) return null;
        return new Session
        {
            Id = Guid.Parse(x.id),
            UserId = (int)x.user_id,
            Active = (bool)x.active,
            CreatedAt = (DateTime)x.created_at,
            TokenRefresh = (string)x.refresk_token,
            TokenRefreshExpireAt = (DateTime)x.refresk_token_expire_at,
        };
    }

    public async Task<bool> Create(Session data)
    {
        var query = "INSERT INTO sessions (id, user_id, refresk_token, refresk_token_expire_at)"
            + " VALUES(@id, @user_id, @refresk_token, @refresk_token_expire_at)";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id.ToString());
        parameters.Add("user_id", data.UserId);
        parameters.Add("refresk_token", data.TokenRefresh);
        parameters.Add("refresk_token_expire_at", data.TokenRefreshExpireAt);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }

    public async Task<bool> Update(Session data)
    {
        var query = "UPDATE sessions SET active=@active WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id.ToString());
        parameters.Add("active", data.Active);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    /*
    public async Task<bool> Delete(Guid id)
    {
        var query = "DELETE FROM sessions WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id.ToString());
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }
    */
}
