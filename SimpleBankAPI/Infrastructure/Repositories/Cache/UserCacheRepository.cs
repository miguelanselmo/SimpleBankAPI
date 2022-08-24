using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

internal class UserCacheRepository : IUserRepository
{
    private readonly IDbTransaction _dbTransaction;
    private readonly IDistributedCache _cache;
    private const string _caheKey = "user";

    public UserCacheRepository(IDbTransaction dbTransaction, IDistributedCache cache)
    {
        _dbTransaction = dbTransaction;
        _cache = cache;
    }

    public async Task<User?> ReadById(int id)
    {
        var resultCache = await _cache.GetRecordAsync<User>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var query = "SELECT * FROM users WHERE id=@id";
            var parameters = new DynamicParameters();
            parameters.Add("id", id);
            var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            var data = Map(resultDb);
            await _cache.SetRecordAsync(_caheKey + ":" + id, data);
            return data;
        }
        else
            return resultCache;
    }

    public async Task<User?> ReadByName(string name)
    {
        var resultCache = await _cache.GetRecordAsync<User[]>(_caheKey+":*");
        if (resultCache is null)
        {
            var query = "SELECT * FROM users WHERE username=@username";
            var parameters = new DynamicParameters();
            parameters.Add("username", name);
            var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters);
            var data = Map(resultDb);
            await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);
            return data;
        }
        else
            return resultCache.Where(x => x.UserName.Equals(name)).FirstOrDefault();
    }
    public async Task<IEnumerable<User>?> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<User[]>(_caheKey+":*");
        if (resultCache is null)
        {
            var query = "SELECT * FROM users";
            var resultDb = await _dbTransaction.Connection.QueryAsync(query);
            var data = Map(resultDb);
            foreach(User user in resultDb) await _cache.SetRecordAsync(_caheKey + ":" + user.Id, data);
            return data;
        }
        else
            return resultCache;
    }

    private static IEnumerable<User>? Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<User> userList = dataDb.Select(x => new User
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
        });
        return userList;
    }

    private static User? Map(dynamic x)
    {
        if (x is null) return null;
        return new User
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(User data)
    {
        var query = "INSERT INTO users (username, password, full_name, email)"
            + " VALUES(@username,  @password,  @full_name, @email) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("username", data.UserName);
        parameters.Add("password", data.Password);
        parameters.Add("full_name", data.FullName);
        parameters.Add("email", data.Email);
        var result = await _dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, _dbTransaction);
        if (result > 0) { data.Id = result; await _cache.SetRecordAsync(_caheKey+":"+data.Id, data); }
        return result > 0 ? (true, result) : (false, null);
    }

    public async Task<bool> Update(User data)
    {
        var query = "UPDATE users SET username=@username, password=@password, full_name=@full_name" +
            " email=@Email WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id);
        parameters.Add("username", data.UserName);
        parameters.Add("password", data.Password);
        parameters.Add("full_name", data.FullName);
        parameters.Add("email", data.Email);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        await _cache.RemoveAsync(_caheKey + ":" + data.Id);
        await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);        
        return result > 0;
    }
    /*
    public async Task<bool> Delete(int id)
    {
        var query = "DELETE FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        await _cache.RemoveAsync(_caheKey + ":" + id);
        return result > 0;
    }
    */
}
