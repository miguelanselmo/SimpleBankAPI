using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Repositories.SqlDataAccess;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    //private readonly ISqlDataAccess db;
    private readonly IDbTransaction dbTransaction;
    //private const string connectionId = "BankDB";

    public UserRepository(/*ISqlDataAccess db,*/ IDbTransaction dbTransaction)
    {
        //this.db = db;
        this.dbTransaction = dbTransaction;
    }

    public async Task<UserModel?> ReadById(int id)
    {
        var query = "SELECT * FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
            var resultDb = await dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }

    public async Task<UserModel?> ReadByName(string name)
    {
        var query = "SELECT * FROM users WHERE username=@username";
        var parameters = new DynamicParameters();
        parameters.Add("username", name);
            var resultDb = await dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, dbTransaction);
            return Map(resultDb);
    }
    public async Task<IEnumerable<UserModel>> ReadAll()
    {
        var query = "SELECT * FROM users";
            var resultDb = await dbTransaction.Connection.QueryAsync(query);
            return Map(resultDb);
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

            var result = await dbTransaction.Connection.ExecuteScalarAsync<int>(query, parameters, dbTransaction);
            return result > 0 ? (true, result) : (false, null);
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

        using (var connection = dbTransaction.Connection)
        {
            var result = await connection.ExecuteAsync(query, parameters, dbTransaction);
            return result > 0;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM users WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using (var connection = dbTransaction.Connection)
        {
            var result = await connection.ExecuteAsync(query, parameters, dbTransaction);
            return result > 0;
        }
    }
}
