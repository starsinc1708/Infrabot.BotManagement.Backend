using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Domain.Repositories;
public class TgBotModuleRepository(
        BotManagementDbContext dbContext,
        IDistributedCache cache,
        ILogger<TgBotModuleRepository> logger
    ) : CachedRepository<ProcessingModule>(dbContext, cache, logger)
{
    private readonly BotManagementDbContext _dbContext = dbContext;
    protected override string CachePrefix => "bot_module";
    
    public async Task<ProcessingModule?> GetWithSettings(long moduleId, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(moduleId);
        return await GetCachedOrLoadAsync(cacheKey, async () =>
        {
            return await _dbContext.Modules
                .Include(m => m.UpdateSettings)
                .FirstOrDefaultAsync(module => module.Id == moduleId, cancellationToken);
        }, cancellationToken);
    }

    public async Task<ProcessingModule?> AddSettings(long moduleId, BotModuleDto.ModuleSettingsOperationDto request, CancellationToken cancellationToken)
    {
        var dbModule = await GetWithSettings(moduleId, cancellationToken);
        if (dbModule == null)
        {
            logger.LogWarning("Module with ID {moduleId} not found", moduleId);
            return null;
        }

        var newIds = request.UpdateSettingIds
            .Where(settingId => !dbModule.UpdateSettings
                .Select(us => us.Id)
                .Contains(settingId))
            .ToList();

        if (newIds.Count == 0)
        {
            logger.LogWarning("Settings with IDs [{settingIds}] already exists for module {BotId}", string.Join(",", request.UpdateSettingIds), moduleId);
            return dbModule;
        }

        var moduleSettingsArr = newIds.Select(id => new ModuleUpdateSettings()
        {
            ModuleId = moduleId,
            UpdateSettingsId = id
        }).ToArray();
        
        dbModule.UpdateSettings.AddRange(moduleSettingsArr);
        await _dbContext.TgModuleUpdateSettings.AddRangeAsync(moduleSettingsArr, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await InvalidateCacheAsync(GetCacheKey(moduleId), $"module-update-settings:module-id:{dbModule.Id}");
        
        return await GetWithSettings(moduleId, cancellationToken);
    }

    public async Task<ProcessingModule?> RemoveSettings(long moduleId, BotModuleDto.ModuleSettingsOperationDto request, CancellationToken cancellationToken)
    {
        var dbModule = await GetWithSettings(moduleId, cancellationToken);
        if (dbModule == null)
        {
            logger.LogWarning("Module with ID {moduleId} not found", moduleId);
            return null;
        }

        var settingsToDelete = dbModule.UpdateSettings
            .Where(s => request.UpdateSettingIds.Contains(s.UpdateSettingsId))
            .ToArray();
        
        dbModule.UpdateSettings.RemoveAll(s => request.UpdateSettingIds.Contains(s.UpdateSettingsId));

        if (settingsToDelete.Length == 0)
        {
            logger.LogWarning("Settings not found for module {moduleid}", moduleId);
            return dbModule;
        }
        _dbContext.TgModuleUpdateSettings.RemoveRange(settingsToDelete);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await InvalidateCacheAsync(GetCacheKey(moduleId), $"module-update-settings:module-id:{dbModule.Id}");
        
        return await GetWithSettings(moduleId, cancellationToken);
    }
    
    public async Task<List<long>> GetByUpdateSettingsIdAsync(long updateSettingsId, CancellationToken cancellationToken)
    {    
        return await _dbContext.TgModuleUpdateSettings
            .AsNoTracking()
            .Where(x => x.UpdateSettingsId == updateSettingsId && x.Module != null)
            .Select(x => x.Module!.Id)
            .ToListAsync(cancellationToken);
    }
}