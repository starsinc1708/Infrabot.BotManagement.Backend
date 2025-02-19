using Infrabot.BotManagement.Domain.Models;
using Infrabot.BotManagement.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.BotManagement.Domain.Endpoints
{
    public abstract class UpdateSettingsEndpoints
    {
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/update-settings").WithTags(nameof(UpdateSettings));
            
            group.MapGet("/", async (
                [FromServices] UpdateSettingsRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var settings = await repo.GetAll(cancellationToken);
                return settings.Count != 0 ? Results.Ok(settings) : Results.NotFound();
            });
            
            group.MapGet("/{id:long}", async (
                long id, 
                [FromServices] UpdateSettingsRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var setting = await repo.GetById(id, cancellationToken);
                return setting is not null ? Results.Ok(setting) : Results.NotFound();
            });
            
            group.MapGet("/update-source/{sourceId:long}", async (
                long sourceId,
                [FromServices] UpdateSettingsRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var settings = await repo.GetBySourceId(sourceId, cancellationToken);
                return settings.Count != 0 ? Results.Ok(settings) : Results.NotFound();
            });
            
            group.MapGet("/update-type/{typeId:long}", async (
                long typeId,
                [FromServices] UpdateSettingsRepository repo, 
                CancellationToken cancellationToken) =>
            {
                var settings = await repo.GetByTypeId(typeId, cancellationToken);
                return settings.Count != 0 ? Results.Ok(settings) : Results.NotFound();
            });
        }
    }
}