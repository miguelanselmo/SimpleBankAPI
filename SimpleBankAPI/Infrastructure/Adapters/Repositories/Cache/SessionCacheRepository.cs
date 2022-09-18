using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Infrastructure.Ports.Repositories;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Cache;

internal class SessionCacheRepository : ISessionRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private const string _caheKey = "Session";

    public SessionCacheRepository(IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Session?> ReadById(Guid id)
    {
        var resultCache = await _cache.GetRecordAsync<Session>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var data = await _unitOfWork.SessionRepository.ReadById(id);
            await _cache.SetRecordAsync(_caheKey + ":" + id, data);
            return data;
        }
        else
            return resultCache;
    }

    public async Task<bool> Create(Session data)
    {
        var result = await _unitOfWork.SessionRepository.Create(data);
        if (result) { await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data); }
        return result;
    }

    public async Task<bool> Update(Session data)
    {
        var result = await _unitOfWork.SessionRepository.Update(data);
        if (result)
        {
            await _cache.RemoveAsync(_caheKey + ":" + data.Id);
            await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);
        }
        return result;
    }
}
