using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories.Cache;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class UserCacheRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "UserDB";
    private const string _caheKey = "User";


    public UserCacheRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> DeleteUser(int id)
    {

        var query = "DELETE FROM Users WHERE Id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey);
                return true;
            }
            return false;
        }
    }

    public async Task<UserModel?> GetUser(int id)
    {
        var resultCache = _cache.GetRecordAsync<UserModel[]>(_caheKey);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM Users WHERE Id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                return await connection.QueryFirstOrDefaultAsync<UserModel>(query, parameters);
            }
        }
        else
        {
            return resultCache.Result.Where(x => x.Id.Equals(id)).FirstOrDefault();
        }
    }

    public async Task<IEnumerable<UserModel>> GetUsers()
    {
        var resultCache = _cache.GetRecordAsync<UserModel[]>(_caheKey);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM Users";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync<UserModel>(query);
                await _cache.SetRecordAsync(_caheKey, resultDb);
                return resultDb.ToList();
            }
        }
        else
            return resultCache.Result;
    }

    public async Task<bool> InsertUser(UserModel User)
    {
        var query = "INSERT INTO users (username, hashed_password, full_name, email, created_at)"
            + " VALUES(@username,  @hashed_password,  @FullName, @Email, @CreatedAt)";
        var parameters = new DynamicParameters();
        parameters.Add("Id", User.Id);
        parameters.Add("UserName", User.UserName);
        parameters.Add("Password", User.Password);
        parameters.Add("FullName", User.FullName);
        parameters.Add("Email", User.Email);
        parameters.Add("CreatedAt", User.CreatedAt);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> UpdateUser(UserModel User)
    {
        var query = "UPDATE users SET username=@username, hashed_password=@hashed_password, full_name=@FullName" +
            ", email=@Email, created_at=@CreatedAt WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", User.Id);
        parameters.Add("UserName", User.UserName);
        parameters.Add("Password", User.Password);
        parameters.Add("FullName", User.FullName);
        parameters.Add("Email", User.Email);
        parameters.Add("CreatedAt", User.CreatedAt);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey);
                return true;
            }
            return false;
        }
    }
}
