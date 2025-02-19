using Infrabot.BotManagement.Domain.Models;
using Infrabot.BotManagement.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.BotManagement.Domain.Endpoints;

public abstract class ModuleUpdateSettingsEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/module-settings").WithTags(nameof(ModuleUpdateSettings));
            
        group.MapGet("/", async (
            [FromServices] ModuleUpdateSettingsRepository repo, 
            CancellationToken cancellationToken) =>
        {
            var settings = await repo.GetAll(cancellationToken);
            return settings.Count != 0 ? Results.Ok(settings) : Results.NotFound();
        });
            
        group.MapGet("/{id:long}", async (
            long id, 
            [FromServices] ModuleUpdateSettingsRepository repo, 
            CancellationToken cancellationToken) =>
        {
            var setting = await repo.GetById(id, cancellationToken);
            return setting is not null ? Results.Ok(setting) : Results.NotFound();
        });
            
        group.MapGet("/module/{moduleId:long}", async (
            long moduleId,
            [FromServices] ModuleUpdateSettingsRepository repo, 
            CancellationToken cancellationToken) =>
        {
            var settings = await repo.GetByModuleIdAsync(moduleId, cancellationToken);
            return settings.Count != 0 ? Results.Ok(settings) : Results.NotFound();
        });
    }
}