using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Repositories.Cache;

internal class AccountCacheRepository : IAccountRepository
{
    
    private readonly IDistributedCache _cache;
    private const string _caheKey = "Account";
    private readonly IUnitOfWork _unitOfWork;

    public AccountCacheRepository(IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Account?> ReadById(int userId, int id)
    {
        var resultCache = await _cache.GetRecordAsync<Account[]>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var data = await _unitOfWork.AccountRepository.ReadById(userId, id);
            await _cache.SetRecordAsync(_caheKey+id, data);
            return data;
        }
        else
            return resultCache.Where(x => x.UserId.Equals(userId)).FirstOrDefault();
    }

    public async Task<Account?> ReadById(int id)
    {
        var resultCache = await _cache.GetRecordAsync<Account[]>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var data = await _unitOfWork.AccountRepository.ReadById(id);
            await _cache.SetRecordAsync(_caheKey + id, data);
            return data;
        }
        else
            return resultCache.Where(x => x.Id.Equals(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<Account>?> ReadByUser(int userId)
    {
        var cache = await _cache.GetRecordAsync<Account[]>(_caheKey + ":" + "*");
        var resultCache = cache.Where(x => x.UserId.Equals(userId));
        if (resultCache is null)
        {
            var data = await _unitOfWork.AccountRepository.ReadByUser(userId);
            foreach (Account account in data) await _cache.SetRecordAsync(_caheKey + ":" + account.Id, account);
            return data;
        }
        else
            return resultCache.Where(x => x.UserId.Equals(userId));
    }
    public async Task<(bool, int?)> Create(Account data)
    {
        var result = await _unitOfWork.AccountRepository.Create(data);
        if (result.Item1) { data.Id = (int)result.Item2; await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data); }
        return result;
    }

    public async Task<bool> Update(Account data)
    {
        var result = await _unitOfWork.AccountRepository.Update(data);
        if (result)
        {
            await _cache.RemoveAsync(_caheKey + ":" + data.Id);
            await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data);
        }
        return result;
    }
}
