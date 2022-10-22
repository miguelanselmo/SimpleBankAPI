namespace SimpleBankAPI.Infrastructure.Ports.Repositories;

public interface IFileRepository
{
    string GetUniqueFileName(string subFolder, string fileNameOrig, string fileNameId);
    Task<(bool, string?, Document?)> SaveDocument(Document document);
    //Task<(bool, string?, Document?)> GetDocument(Document document);
    Task<(bool, string?, FileStreamResult?)> GetDocument(Document document);
}
