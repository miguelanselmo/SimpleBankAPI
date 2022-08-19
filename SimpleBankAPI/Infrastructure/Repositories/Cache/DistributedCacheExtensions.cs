using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SimpleBankAPI.Infrastructure.Repositories;

public static class DistributedCacheExtensions
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
        string cacheKey,
        T data,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions();

        options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60);
        options.SlidingExpiration = unusedExpireTime;

        var jsonData = JsonSerializer.Serialize(data);
        await cache.SetStringAsync(cacheKey, jsonData, options);
    }

    public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string cacheKey)
    {
        var jsonData = await cache.GetStringAsync(cacheKey);

        if (jsonData is null)
        {
            return default(T);
        }

        return JsonSerializer.Deserialize<T>(jsonData);
    }
}