using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Domain.Repositories;

public class ModuleUpdateSettingsRepository(
    BotManagementDbContext dbContext,
    IDistributedCache cache,
    ILogger<ModuleUpdateSettingsRepository> logger
    ) : CachedRepository<ModuleUpdateSettings>(dbContext, cache, logger)
{
    private readonly BotManagementDbContext _dbContext = dbContext;
    protected override string CachePrefix => "module-update-settings";
    
    public async Task<List<ModuleUpdateSettings>> GetByModuleIdAsync(long moduleId, CancellationToken cancellationToken)
    {
        var cacheKey = GetCustomKey("module-id", moduleId);
        
        return await GetCachedListOrLoadAsync(cacheKey, async () =>
        {
            return await _dbContext.TgModuleUpdateSettings
                .Where(x => x.ModuleId == moduleId)
                .Include(x => x.Module)
                .Include(x => x.UpdateSettings)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }
}