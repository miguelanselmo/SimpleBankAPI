using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBankAPI.Core.Entities;
using SimpleBankAPI.Infrastructure.Ports.Repositories;
using SimpleBankAPI.Infrastructure.Adapters.Repositories.Mapper;
using System.Data;

namespace SimpleBankAPI.Infrastructure.Adapters.Repositories.Cache;

internal class MovementCacheRepository : IMovementRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private const string _caheKey = "Movement";

    public MovementCacheRepository(IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Movement?> ReadById(int accountId, int id)
    {
        var resultCache = await _cache.GetRecordAsync<Movement[]>(_caheKey + ":" + id);
        if (resultCache is null)
        {
            var data = await _unitOfWork.MovementRepository.ReadById(accountId, id);
            await _cache.SetRecordAsync(_caheKey + id, data);
            return data;
        }
        else
            return resultCache.Where(x => x.AccountId.Equals(accountId)).FirstOrDefault();
    }

    public async Task<IEnumerable<Movement>?> ReadByAccount(int accountId)
    {
        var cache = await _cache.GetRecordAsync<Movement[]>(_caheKey + ":" + "*");
        var resultCache = cache.Where(x => x.AccountId.Equals(accountId));
        if (resultCache is null)
        {
            var data = await _unitOfWork.MovementRepository.ReadByAccount(accountId);
            foreach (Movement mov in data) await _cache.SetRecordAsync(_caheKey + ":" + mov.Id, mov);
            return data;
        }
        else
            return resultCache;
    }


    public async Task<(bool, int?)> Create(Movement data)
    {
        var result = await _unitOfWork.MovementRepository.Create(data);
        if (result.Item1) { data.Id = (int)result.Item2; await _cache.SetRecordAsync(_caheKey + ":" + data.Id, data); }
        return result;
    }

    public Task<(bool, int?)> CreateLog(Transfer data)
    {
        return _unitOfWork.MovementRepository.CreateLog(data);
    }
}
