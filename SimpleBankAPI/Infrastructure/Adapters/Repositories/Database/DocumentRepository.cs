using Dapper;
using SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Database;

internal class DocumentRepository : IDocumentRepository
{
    private readonly IDbTransaction _dbTransaction;

    public DocumentRepository(IDbTransaction dbTransaction)
    {
        _dbTransaction = dbTransaction;
    }

    public async Task<Document?> ReadById(int accountId, string id)
    {
        var query = "SELECT * FROM documents WHERE id=@id AND account_id=@account_id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("account_id", accountId);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, _dbTransaction);
        return DocumentMapper.Map(resultDb);
    }

    public async Task<Document?> ReadById(string id)
    {
        var query = "SELECT * FROM documents WHERE id=@id";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        var resultDb = await _dbTransaction.Connection.QueryFirstOrDefaultAsync<object>(query, parameters, _dbTransaction);
        return DocumentMapper.Map(resultDb);
    }

    public async Task<IEnumerable<Document>?> ReadByAccount(int accountId)
    {
        var query = "SELECT * FROM documents WHERE account_id=@account_id";
        var parameters = new DynamicParameters();
        parameters.Add("account_id", accountId);
        var resultDb = await _dbTransaction.Connection.QueryAsync<object>(query, parameters, _dbTransaction);
        return DocumentMapper.Map(resultDb);
    }
    
    public async Task<bool> Create(Document data)
    {
        var query = "INSERT INTO documents (id, account_id, file_name, uri, content_type)"
            + " VALUES(@id, @account_id, @file_name, @uri, @content_type)";
        var parameters = new DynamicParameters();
        parameters.Add("id", data.Id);
        parameters.Add("account_id", data.AccountId);
        parameters.Add("file_name", data.FileName);
        parameters.Add("uri", data.URI);
        parameters.Add("content_type", data.ContentType);
        var result = await _dbTransaction.Connection.ExecuteAsync(query, parameters, _dbTransaction);
        return result > 0;
    }    
}
