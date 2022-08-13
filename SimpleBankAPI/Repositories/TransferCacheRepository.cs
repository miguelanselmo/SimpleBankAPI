﻿using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Models;
using SimpleBankAPI.Models.Enums;
using SimpleBankAPI.Repositories.Cache;
using SimpleBankAPI.Repositories.SqlDataAccess;

namespace SimpleBankAPI.Repositories;

public class TransferCacheRepository : ITransferRepository
{
    private readonly ISqlDataAccess _db;
    private readonly IDistributedCache _cache;
    private const string _connectionId = "BankDB";
    private const string _caheKey = "Transfer";


    public TransferCacheRepository(ISqlDataAccess db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<TransferModel?> Read(int id)
    {
        var resultCache = _cache.GetRecordAsync<TransferModel[]>(_caheKey+id);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM transfers WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryFirstOrDefaultAsync<object>(query, parameters);
                //var resultDb = await connection.QueryAsync<object>(query, parameters);
                TransferModel Transfer = Map(resultDb);
                await _cache.SetRecordAsync(_caheKey+id, Transfer);
                return Transfer;
            }
        }
        else
            return resultCache.Result.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<TransferModel>> Read()
    {
        var resultCache = _cache.GetRecordAsync<TransferModel[]>(_caheKey);
        if (resultCache.Result is null)
        {
            var query = "SELECT * FROM transfers";
            using (var connection = _db.GetSqlConnection(_connectionId))
            {
                var resultDb = await connection.QueryAsync(query);
                return Map(resultDb);
            }
        }
        else
            return Map(resultCache.Result);
    }

    private static IEnumerable<TransferModel> Map(IEnumerable<dynamic> TransferDb)
    {
    IEnumerable<TransferModel> TransferList = TransferDb.Select(x => new TransferModel
        {
            Id = (int)x.id,
            FromAccountId = (int)x.from_account_id,
            ToAccountId = (int)x.from_account_id,
            Amount = (decimal)x.amount,
            CreatedAt = (DateTime)x.created_at,
        });
        return TransferList;
    }

    private static TransferModel Map(dynamic x)
    {
        return new TransferModel
        {
            Id = (int)x.id,
            FromAccountId = (int)x.from_account_id,
            ToAccountId = (int)x.to_account_id,
            Amount = (decimal)x.amount,
            CreatedAt = (DateTime)x.created_at,
        };
    }
    
    public async Task<bool> Create(TransferModel Transfer)
    {
        var query = "INSERT INTO Transfers (from_account_id, to_account_id, amount)"
            + " VALUES(@FromAccountId,  @ToAccountId,  @Amount)";
        var parameters = new DynamicParameters();
        parameters.Add("FromAccountId", Transfer.FromAccountId);
        parameters.Add("ToAccountId", Transfer.ToAccountId);
        parameters.Add("Amount", Transfer.Amount);

        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.SetRecordAsync(_caheKey + Transfer.Id, Transfer);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Update(TransferModel Transfer)
    {
        var query = "UPDATE transfers SET amount=@Amount" +
            ", WHERE id=@Id";
        var parameters = new DynamicParameters();
        parameters.Add("Id", Transfer.Id);
        parameters.Add("Amount", Transfer.Amount);
        
        using (var connection = _db.GetSqlConnection(_connectionId))
        {
            var result = await connection.ExecuteAsync(query, parameters);
            if (result > 0)
            {
                await _cache.RemoveAsync(_caheKey+ Transfer.Id);
                await _cache.SetRecordAsync(_caheKey + Transfer.Id, Transfer);
                return true;
            }
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {

        var query = "DELETE FROM transfers WHERE id=@Id";
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