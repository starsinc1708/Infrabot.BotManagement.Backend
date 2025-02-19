using System.Text.Json;
using Infrabot.BotManagement.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Domain.Repositories;

public abstract class CachedRepository<TEntity>(
    BotManagementDbContext dbContext, 
    IDistributedCache cache,
    ILogger<CachedRepository<TEntity>> logger) 
    : BaseRepository<TEntity>(dbContext) where TEntity : BaseModel
{
    private readonly ILogger _logger = logger;
    protected abstract string CachePrefix { get; }
    protected virtual TimeSpan CacheDuration => TimeSpan.FromMinutes(120);
    
    protected string GetCacheKey(object id) => $"{CachePrefix}:id:{id}";
    
    protected string GetCustomKey(string keyType, object value) => $"{CachePrefix}:{keyType}:{value}";
    
    protected async Task<TEntity?> GetCachedOrLoadAsync(
        string cacheKey, 
        Func<Task<TEntity?>> loadFromDb,
        CancellationToken ct)
    {
        try
        {
            var cached = await cache.GetStringAsync(cacheKey, ct);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<TEntity>(cached);
            }

            var data = await loadFromDb();
            if (data != null)
            {
                await cache.SetStringAsync(
                    cacheKey, 
                    JsonSerializer.Serialize(data), 
                    new DistributedCacheEntryOptions { SlidingExpiration = CacheDuration }, 
                    ct);
            }
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache operation failed for {CacheKey}", cacheKey);
            return await loadFromDb();
        }
    }
    
    protected async Task<List<TEntity>> GetCachedListOrLoadAsync(
        string cacheKey,
        Func<Task<List<TEntity>>> loadFromDb,
        CancellationToken ct)
    {
        try
        {
            var cached = await cache.GetStringAsync(cacheKey, ct);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<List<TEntity>>(cached) ?? [];
            }
            
            var data = await loadFromDb();
            if (data.Count != 0)
            {
                await cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(data),
                    new DistributedCacheEntryOptions { SlidingExpiration = CacheDuration },
                    ct);
            }
            
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache operation failed for {CacheKey}", cacheKey);
            return await loadFromDb();
        }
    }
    
    protected async Task InvalidateCacheAsync(params string[] keys)
    {
        foreach (var key in keys)
        {
            await cache.RemoveAsync(key);
        }
    }
    
    public virtual async Task<TEntity?> GetByIdCached(long id, CancellationToken ct)
        => await GetCachedOrLoadAsync(
            GetCacheKey(id),
            () => base.GetById(id, ct),
            ct);
}