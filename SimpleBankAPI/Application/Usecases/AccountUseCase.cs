using SimpleBankAPI.Application.Interfaces;
using SimpleBankAPI.Infrastructure.Ports.Repositories;

namespace SimpleBankAPI.Application.Usecases;

public class AccountUseCase : IAccountUseCase
{
    private readonly ILogger<AccountUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly IFileRepository _fileRepository;

    public AccountUseCase(ILogger<AccountUseCase> logger, IUnitOfWork unitOfWork, IConfiguration config, IFileRepository fileRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _config = config;
        _fileRepository = fileRepository;
    }
    
    public async Task<(ErrorTypeUsecase?, string?, Account?)> CreateAccount(Account account)
    {
        bool commit = false;
        try
        {
            var result = await _unitOfWork.AccountRepository.Create(account);
            if (result.Item1)
            {
                commit = true;
                account.Id = (int)result.Item2;
                return (null, null, account);
            }
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError), null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(ErrorTypeUsecase?, string?, IEnumerable<Account>?)> GetAccounts(int userId)
    {
        try
        {
            var result = await _unitOfWork.AccountRepository.ReadByUser(userId);
            if (result is not null)
                return (null, null, result);
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.AccountNotFound), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountReadError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.AccountReadError), null);
        }
    }

    public async Task<(ErrorTypeUsecase?, string?, Account?, IEnumerable<Movement>?)> GetAccountMovements(int userId, int id)
    {
        try
        {
            var result = await _unitOfWork.AccountRepository.ReadById(userId, id);
            if (result is not null)
            {
                return (null, null, result, await _unitOfWork.MovementRepository.ReadByAccount(id));
            }
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.AccountNotFound), null, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountMovementReadError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.AccountMovementReadError), null, null);
        }
    }


    public async Task<(ErrorTypeUsecase?, string?, Document?)> SaveDocument(Document document)
    {
        bool commit = false;
        try
        {
            //TODO: validar content type
            document.ContentType = document.Content.ContentType;
            document.Id = Guid.NewGuid();
            document.FileName = document.Content.FileName;
            document.URI = _fileRepository.GetUniqueFileName("", document.FileName, document.Id.ToString());
            document.CreatedAt = DateTime.Now;
            var result = await _unitOfWork.DocumentRepository.Create(document);
            if (result)
            {
                var resultFile = await _fileRepository.SaveDocument(document);
                if (resultFile.Item1) { 
                    commit = true;
                    return (null, null, document);
                }
                else
                    return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentCreatError), null);
            }
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentCreatError), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.AccountCreatError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentCreatError), null);
        }
        finally
        {
            if (commit) _unitOfWork.Commit(); else _unitOfWork.Rollback();
        }
    }

    public async Task<(ErrorTypeUsecase?, string?, IEnumerable<Document>?)> ListDocuments(int id)
    {
        try
        {
            var result = await _unitOfWork.DocumentRepository.ReadByAccount(id);
            if (result is not null)
                return (null, null, result);
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentNotFound), null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentReadError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentReadError), null);
        }
    }

    public async Task<(ErrorTypeUsecase?, string?, Document?, FileStreamResult?)> GetDocument(int id, string docId)
    {
        try
        {
            var result = await _unitOfWork.DocumentRepository.ReadById(id, docId);
            if (result is not null)
            {
                var resultFile = await _fileRepository.GetDocument(result);
                return (null, null, result, resultFile.Item3);
            }
            else
                return (ErrorTypeUsecase.Business, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentNotFound), null, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentReadError));
            return (ErrorTypeUsecase.System, EnumHelper.GetEnumDescription(ErrorUsecase.DocumentReadError), null, null);
        }
    }
}
