using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpdateSource = Infrabot.BotManagement.Domain.Enums.UpdateSource;
using UpdateType = Telegram.Bot.Types.Enums.UpdateType;

namespace Infrabot.BotManagement.Domain.Repositories;

public class UpdateSettingsRepository(
    BotManagementDbContext dbContext,
    IDistributedCache cache,
    ILogger<UpdateSettingsRepository> logger
    ) : CachedRepository<UpdateSettings>(dbContext, cache, logger)
{
    private readonly BotManagementDbContext _dbContext = dbContext;
    protected override string CachePrefix => "update_settings";
    
    public async Task<List<UpdateSettings>> GetBySourceId(long sourceId, CancellationToken ct)
    {
        var cacheKey = GetCustomKey("source", sourceId);
        return await GetCachedListOrLoadAsync(cacheKey, async () =>
        {
            return await _dbContext.UpdateSettings
                .Where(s => s.UpdateSource == (UpdateSource)sourceId)
                .AsNoTracking()
                .Include(x => x.ModuleSettings)
                .ToListAsync(ct);
        }, ct);
    }
    
    public async Task<List<UpdateSettings>> GetByTypeId(long typeId, CancellationToken ct)
    {
        var cacheKey = GetCustomKey("type", typeId);
        return await GetCachedListOrLoadAsync(cacheKey, async () =>
        {
            return await _dbContext.UpdateSettings
                .Where(s => s.UpdateType == (UpdateType)typeId)
                .AsNoTracking()
                .Include(x => x.ModuleSettings)
                .ToListAsync(ct);
        }, ct);
    }

    public async Task<UpdateSettings?> GetBySourceAndType(UpdateSource updateSource, UpdateType updateType, CancellationToken cancellationToken)
    {
        var cacheKey = GetCustomKey("source-and-type", $"{updateSource}-{updateType}");
        return await GetCachedOrLoadAsync(cacheKey, async () =>
        {
            return await _dbContext.UpdateSettings
                .Where(s => s.UpdateType == updateType && s.UpdateSource == updateSource)
                .AsNoTracking()
                .Include(x => x.ModuleSettings)
                    .ThenInclude(ms => ms.Module)
                .FirstOrDefaultAsync(cancellationToken);
        }, cancellationToken);
    }
}

