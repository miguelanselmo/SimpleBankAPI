using SimpleBankAPI.Infrastructure.Adapters.Providers;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using System.IO;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.File;

public class FileRepository : IFileRepository
{

    private readonly ILogger<AuthenticationProvider> _logger;
    private readonly IConfiguration _config;
    private readonly string _fileRepoPath;


    public FileRepository(ILogger<AuthenticationProvider> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _fileRepoPath = _config["FileRepo:Path"];
    }

    public string GetUniqueFileName(string subFolder, string fileNameOrig, string fileNameId)
    {
        var dirPath = Path.Combine(_fileRepoPath, subFolder);
        return Path.Combine(dirPath, fileNameId + Path.GetExtension(fileNameOrig));
    }


    public async Task<(bool, string?, Document?)> SaveDocument(Document document)
    {
        try
        {
            using (var stream = new FileStream(document.URI, FileMode.Create))
            {
                await document.Content.CopyToAsync(stream);
            }

            return (true, null, document);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }
    
    public async Task<(bool, string?, FileStreamResult?)> GetDocument(Document document)
    {
        try
        {
            var stream = System.IO.File.OpenRead(document.URI);
            return (true, null, new FileStreamResult(stream, document.ContentType));
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }
}
    

