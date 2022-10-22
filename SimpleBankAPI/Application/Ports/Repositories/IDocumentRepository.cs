namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface IDocumentRepository
{
    Task<Document?> ReadById(int accountId, string id);
    Task<Document?> ReadById(string id);
    Task<IEnumerable<Document>?> ReadByAccount(int accountId);
    Task<bool> Create(Document data);
}
