
namespace SimpleBankAPI.Application.Interfaces;

public interface IAccountUseCase
{
    Task<(ErrorTypeUsecase?, string?, Account?)> CreateAccount(Account user);

    Task<(ErrorTypeUsecase?, string?, IEnumerable<Account>?)> GetAccounts(int userId);

    Task<(ErrorTypeUsecase?, string?, Account?, IEnumerable<Movement>?)> GetAccountMovements(int userId, int id);
    Task<(ErrorTypeUsecase?, string?, Document?)> SaveDocument(Document document);

    Task<(ErrorTypeUsecase?, string?, IEnumerable<Document>?)> ListDocuments(int id);

    Task<(ErrorTypeUsecase?, string?, Document?, FileStreamResult?)> GetDocument(int id, string docId);
}
