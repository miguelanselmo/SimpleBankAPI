using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Repositories.Cache;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class UserCacheRepository : IUserRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "User";


    public UserCacheRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<UserModel?> Read(int id)
    {
        var resultCache = _cache.GetRecordAsync<UserModel[]>(_caheKey+id);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM users WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                UserModel user = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey+id, user);
                return user;
            }
        }
        else
            return resultCache.Result.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<UserModel>> Read()
    {
        var resultCache = _cache.GetRecordAsync<UserModel[]>(_caheKey);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM users";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                return Map(resultDb);
            }
        }
        else
            return Map(resultCache.Result);
    }

    private static IEnumerable<UserModel> Map(IEnumerable<dynamic> userDb)
    {
        IEnumerable<UserModel> userList = userDb.Select(x => new UserModel
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.hashed_password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
            PasswordChangedAt = (DateTime)x.password_changed_at
        });
        return userList;
    }

    private static UserModel Map(dynamic x)
    {
        return new UserModel
        {
            Id = (int)x.id,
            UserName = (string)x.username,
            Email = (string)x.email,
            Password = (string)x.hashed_password,
            FullName = (string)x.full_name,
            CreatedAt = (DateTime)x.created_at,
            PasswordChangedAt = (DateTime)x.password_changed_at
        };
    }
    
    public async Task<bool> Create(UserModel user)
    {
        var query = "INSERT INTO users (username, hashed_password, full_name, email, created_at)"
            + " VALUES(@username,  @hashed_password,  @FullName, @Email, @CreatedAt)";
        var parameters = new DynamicParameters();
        parameters.Add("Id", user.Id);
        parameters.Add("UserName", user.UserName);
        parameters.Add("Password", user.Password);
        parameters.Add("FullName", user.FullName);
        parameters.Add("Email", user.Email);
        parameters.Add("CreatedAt", user.CreatedAt);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.SetRecordAsync(_caheKey + user.Id, user);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Update(UserModel user)
    {
        var query = "UPDATE users SET username=@username, hashed_password=@hashed_password, full_name=@FullName" +
            ", email=@Email, created_at=@CreatedAt WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", user.Id);
        parameters.Add("UserName", user.UserName);
        parameters.Add("Password", user.Password);
        parameters.Add("FullName", user.FullName);
        parameters.Add("Email", user.Email);
        parameters.Add("CreatedAt", user.CreatedAt);
        
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey+ user.Id);
                await _cache.SetRecordAsync(_caheKey + user.Id, user);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM users WHERE id=@Id";
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
