using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Models;
using Infrabot.BotManagement.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.BotManagement.Domain.Endpoints
{
    public abstract class TelegramBotEndpoints
    {
        public static void Map(WebApplication app) 
        {
            var group = app.MapGroup("/bot").WithTags(nameof(TelegramBot));

            group.MapGet("/", async (
                [FromServices] TelegramBotRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var bot = (await repo.GetAll(cancellationToken)).ToList();
                return bot.Count != 0 ? Results.Ok(bot) : Results.NotFound();
            });

            group.MapGet("/{id:long}", async (
                long id,
                [FromServices] TelegramBotRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var bot = await repo.GetById(id, cancellationToken);
                return bot is null ? Results.NotFound() : Results.Ok(bot);
            });
            
            group.MapGet("/{id:long}/module", async (
                long id,
                [FromServices] TelegramBotRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var bot = await repo.GetWithModules(id, cancellationToken);
                return bot is null ? Results.NotFound() : Results.Ok(bot);
            });
            
            group.MapPost("/{id:long}/module", async (
                long id,
                [FromBody] BotModuleDto.ModuleBotOperationDto modules,
                [FromServices] TelegramBotRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var bot = await repo.AddModules(id, modules, cancellationToken);
                return bot is null ? Results.NotFound() : Results.Ok(bot);
            });
            
            group.MapDelete("/{id:long}/module", async (
                long id,
                [FromBody] BotModuleDto.ModuleBotOperationDto modules,
                [FromServices] TelegramBotRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var bot = await repo.RemoveModules(id, modules, cancellationToken);
                return bot is null ? Results.NotFound() : Results.Ok(bot);
            });
        }
    }
}