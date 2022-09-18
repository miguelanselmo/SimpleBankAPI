using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Cache;

internal class UserCacheRepository : IUserRepository
{
    private readonly IDistributedCache _cache;
    private const string _caheKey = "user";
    private readonly IUnitOfWork _unitOfWork;

    public UserCacheRepository(IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<User?> ReadById(int id)
    {
        var resultCache = await _cache.GetRecordAsync<User>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var data = await _unitOfWork.UserRepository.ReadById(id);
            await _cache.SetRecordAsync(_caheKey + ":" + id, data);
            return data;
        }
        else
            return resultCache;
    }

    public async Task<User?> ReadByName(string name)
    {
        var cache = await _cache.GetRecordAsync<User[]>(_caheKey + ":*");
        var resultCache = cache.Where(x => x.UserName.Equals(name));
        if (resultCache is null)
        {
            var data = await _unitOfWork.UserRepository.ReadByName(name);
            await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);
            return data;
        }
        else
            return resultCache.FirstOrDefault();
    }
    public async Task<IEnumerable<User>?> ReadAll()
    {
        var resultCache = await _cache.GetRecordAsync<User[]>(_caheKey + ":*");
        if (resultCache is null)
        {
            var data = await _unitOfWork.UserRepository.ReadAll();
            foreach (User user in data) await _cache.SetRecordAsync(_caheKey + ":" + user.Id, user);
            return data;
        }
        else
            return resultCache;
    }

    public async Task<(bool, int?)> Create(User data)
    {
        var result = await _unitOfWork.UserRepository.Create(data);
        if (result.Item1) { data.Id = (int)result.Item2; await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data); }
        return result;
    }

    public async Task<bool> Update(User data)
    {
        var result = await _unitOfWork.UserRepository.Update(data);
        if (result)
        {
            await _cache.RemoveAsync(_caheKey + ":" + data.Id);
            await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);
        }
        return result;
    }
}
