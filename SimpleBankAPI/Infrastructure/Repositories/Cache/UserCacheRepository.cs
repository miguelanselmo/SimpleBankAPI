using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories
    ;

public class UserCacheRepository : IUserRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "User";

    public UserCacheRepository(ISqlDataAccess db, IDbTransaction dbTransaction, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<UserModel?> ReadById(int id)
    {
        var resultCache = await _cache.GetRecordAsync<UserModel[]>(_caheKey + id);
        //var resultCache = await _cache.GetRecordAsync<UserModel[]>(_caheKey);
        if (resultCache is null)
        {
            var query = "SELECT * FROM users WHERE id=@id";
            var parameters = new DynamicParameters();
            parameters.Add("id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                UserModel dataModel = Map(resultDb);
                //await _cache.SetRecordAsync(_caheKey+id, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }
    /*
    public async Task<UserModel?> ReadByName(string name)
    {
        var resultCache = await _cache.GetRecordAsync<UserModel[]>(_caheKey+id);
        //var resultCache = await _cache.GetRecordAsync<UserModel[]>(_caheKey);
        if (resultCache is null)
        {
            var query = "SELECT * FROM users WHERE username=@username";
            var parameters = new DynamicParameters();
            parameters.Add("username", name);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
               ///var resultDb = await connection.QueryAsync<object>(query, parameters);
                UserModel dataModel = Map(resultDb);
                //await _cache.SetRecordAsync(_caheKey+id, dataModel);
                return dataModel;
            }
        }
        else
            return resultCache.Where(x => x.UserName.Equals(name)).FirstOrDefault();
    }
    */
    public async Task<IEnumerable<UserModel>> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<UserModel[]>(_caheKey);
        if (resultCache is null)
        {
            var query = "SELECT * FROM users";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                IEnumerable<UserModel> dataModel = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey, dataModel);
                return dataModel;
            }
        }
        else
            return Map(resultCache);
    }

    private static IEnumerable<UserModel> Map(IEnumerable<dynamic> dataDb)
    {
        if (dataDb is null) return null;
        IEnumerable<UserModel> userList = dataDb.Select(x => new UserModel
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

    private static UserModel Map(dynamic x)
    {
        if (x is null) return null;
        return new UserModel
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
        };
    }

    public async Task<(bool, int?)> Create(UserModel dataModel)
    {
        var query = "INSERT INTO users (username, password, full_name, email)"
            + " VALUES(@username,  @password,  @full_name, @email) RETURNING id";
        var parameters = new DynamicParameters();
        parameters.Add("username", dataModel.UserName);
        parameters.Add("password", dataModel.Password);
        parameters.Add("full_name", dataModel.FullName);
        parameters.Add("email", dataModel.Email);

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

    public async Task<bool> Update(UserModel dataModel)
    {
        var query = "UPDATE users SET username=@username, password=@password, full_name=@full_name" +
            ", email=@Email WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", dataModel.Id);
        parameters.Add("username", dataModel.UserName);
        parameters.Add("password", dataModel.Password);
        parameters.Add("full_name", dataModel.FullName);
        parameters.Add("email", dataModel.Email);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                //await _cache.RemoveAsync(_caheKey);
                await _cache.RemoveAsync(_caheKey + dataModel.Id);
                await _cache.SetRecordAsync(_caheKey + dataModel.Id, dataModel);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                //await _cache.RemoveAsync(_caheKey);
                await _cache.RemoveAsync(_caheKey + id);
                return true;
            }
            return false;
        }
    }

    public Task<UserModel?> ReadByName(string name)
    {
        throw new NotImplementedException();
    }
}
