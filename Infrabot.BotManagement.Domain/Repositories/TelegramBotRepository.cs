using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Domain.Repositories;

public class TelegramBotRepository(
    BotManagementDbContext dbContext,
    IDistributedCache cache,
    ILogger<TelegramBotRepository> logger
    ) : CachedRepository<TelegramBot>(dbContext, cache, logger)
{
    private readonly BotManagementDbContext _dbContext = dbContext;
    protected override string CachePrefix => "telegram-bot";
    
    public async Task<TelegramBot?> GetWithModules(long botId, CancellationToken ct)
    {
        var cacheKey = GetCustomKey("bot-id", botId);
        return await GetCachedOrLoadAsync(cacheKey, async () =>
        {
            return await _dbContext.Bots
                .Include(bot => bot.Modules)
                    .ThenInclude(botModule => botModule.Module)
                .FirstOrDefaultAsync(bot => bot.Id == botId, ct);
        }, ct);
    }
    
    public async Task<TelegramBot?> AddModules(long botId, BotModuleDto.ModuleBotOperationDto request,
        CancellationToken ct)
    {
        var dbBot = await GetWithModules(botId, ct);
        if (dbBot == null)
        {
            logger.LogWarning("Bot with ID {BotId} not found", botId);
            return null;
        }
        
        if (dbBot.Modules.Any(m => request.ModuleIds.Contains(m.ModuleId)))
        {
            logger.LogWarning("Module with IDs [{ModuleId}] already exists for bot {BotId}", string.Join(",", request.ModuleIds), botId);
            return dbBot;
        }

        var botModuleArr = request.ModuleIds.Select(moduleId => new BotModule
        {
            ModuleId = moduleId,
            BotId = botId,
            Enabled = true
        });
        
        await _dbContext.TgBotModules.AddRangeAsync(botModuleArr, ct);
        await _dbContext.SaveChangesAsync(ct);
        
        await InvalidateCacheAsync(GetCustomKey("bot-id", botId));
        
        return await GetWithModules(botId, ct);
    }

    public async Task<TelegramBot?> RemoveModules(long botId, BotModuleDto.ModuleBotOperationDto request,
        CancellationToken ct)
    {
        var dbBot = await GetWithModules(botId, ct);
        if (dbBot == null)
        {
            logger.LogWarning("Bot with ID {BotId} not found", botId);
            return null;
        }
        
        var botModules = dbBot.Modules.Where(m => request.ModuleIds.Contains(m.ModuleId)).ToArray();
        if (botModules.Length == 0)
        {
            logger.LogWarning("Modules not found for bot {BotId}", botId);
            return dbBot;
        }
        
        _dbContext.TgBotModules.RemoveRange(botModules);
        await _dbContext.SaveChangesAsync(ct);
        
        await InvalidateCacheAsync(GetCustomKey("bot-id", botId));
        
        return await GetWithModules(botId, ct);
    }
}