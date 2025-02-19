using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Mappings;
using Infrabot.BotManagement.Domain.Models;
using Infrabot.BotManagement.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.BotManagement.Domain.Endpoints;

public abstract class TgBotModuleEndpoints
{
    public static void Map(WebApplication app) 
    {
        var group = app.MapGroup("/botmodule").WithTags(nameof(ProcessingModule));
        
        group.MapGet("/", async (
            [FromServices] TgBotModuleRepository repo, 
            CancellationToken cancellationToken) =>
        {
            var modules = (await repo.GetAll(cancellationToken)).ToList();
            return modules.Count != 0 ? Results.Ok(modules) : Results.NotFound();
        });
        
        group.MapGet("/{id:long}", async (
            long id,
            [FromServices] TgBotModuleRepository repo,
            CancellationToken cancellationToken) =>
        {
            var module = await repo.GetByIdCached(id, cancellationToken);
            return module is not null ? Results.Ok(module) : Results.NotFound();
        });

        group.MapPost("/", async (
            [FromBody] BotModuleDto.CreateProcessingModuleDto entity,
            [FromServices] TgBotModuleRepository repo,
            CancellationToken cancellationToken) =>
        {
            var added = await repo.Add(entity.MapToModel(), cancellationToken);
            return Results.Created($"/botmodules/{added?.Id}", added);
        });
        
        group.MapGet("/{id:long}/settings", async (
            long id,
            [FromServices] TgBotModuleRepository repo,
            CancellationToken cancellationToken) =>
        {
            var module = await repo.GetWithSettings(id, cancellationToken);
            return module is not null ? Results.Ok(module) : Results.NotFound();
        });
        
        group.MapPost("/{id:long}/settings", async (
            long id,
            [FromBody] BotModuleDto.ModuleSettingsOperationDto settings,
            [FromServices] TgBotModuleRepository repo, 
            CancellationToken cancellationToken) =>
        {
            var bot = await repo.AddSettings(id, settings, cancellationToken);
            return bot is null ? Results.NotFound() : Results.Ok(bot);
        });
            
        group.MapDelete("/{id:long}/settings", async (
            long id,
            [FromBody] BotModuleDto.ModuleSettingsOperationDto settings,
            [FromServices] TgBotModuleRepository repo, 
            CancellationToken cancellationToken) =>
        {
            var bot = await repo.RemoveSettings(id, settings, cancellationToken);
            return bot is null ? Results.NotFound() : Results.Ok(bot);
        });
    }
}