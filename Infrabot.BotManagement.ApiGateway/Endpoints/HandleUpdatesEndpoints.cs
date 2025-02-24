using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Infrabot.BotManagement.Broker.Kafka;
using Infrabot.BotManagement.Domain.Grpc;
using Infrabot.BotManagement.Domain.UpdateProcessingUtils;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using UpdateSettings = Infrabot.BotManagement.Domain.Models.UpdateSettings;
using UpdateSource = Infrabot.BotManagement.Domain.Enums.UpdateSource;
using UpdateType = Telegram.Bot.Types.Enums.UpdateType;

namespace Infrabot.BotManagement.ApiGateway.Endpoints;

public static class HandleUpdatesEndpoints
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    public static void MapTelegramUpdatesEndpoints(this WebApplication app)
    {
        app.MapPost("/tgbot/updates", HandleTelegramUpdate);
    }

    private static async Task<IResult> HandleTelegramUpdate(
        Update update,
        [FromServices] UpdateSettingsService.UpdateSettingsServiceClient updateSettingsClient,
        [FromServices] TgBotModuleService.TgBotModuleServiceClient botModuleClient,
        [FromServices] KafkaProducer kafkaProducer,
        ILogger<Update> logger,
        CancellationToken cancellationToken)
    {
        var updateSource = DistributeUpdateHelper.DetermineUpdateSource(update);
        if (IsUnknownUpdate(update, updateSource, logger))
        {
            return Results.BadRequest();
        }

        var updateSetting = await GetUpdateSettingAsync(updateSettingsClient, updateSource, update, logger, cancellationToken);
        if (updateSetting is null)
        {
            return Results.NotFound($"No enabled UpdateSettings found for Source [{updateSource}] and Type [{update.Type}]");
        }

        var activeModules = await GetActiveModulesAsync(botModuleClient, updateSetting.Id, logger, cancellationToken);
        
        await ProduceKafkaMessage(update, updateSetting, updateSource, activeModules, kafkaProducer, "updates_dump", cancellationToken);
        
        if (activeModules.Count == 0)
        {
            return Results.Ok($"No active ProcessingModule found for Source [{updateSource}] and Type [{update.Type}]");
        }
        
        var topic = $"updates_{updateSource}_{update.Type}".ToLowerInvariant();
        await ProduceKafkaMessage(update, updateSetting, updateSource, activeModules, kafkaProducer, topic, cancellationToken);

        return NotifyProcessingResult(update, activeModules.Count, logger);
    }
    
    private static bool IsUnknownUpdate(Update update, UpdateSource updateSource, ILogger logger)
    {
        if (updateSource != UpdateSource.Unknown && update.Type != UpdateType.Unknown) return false;
        logger.LogWarning($"Unknown update type - [{update.Type}]");
        return true;
    }
    
    private static async Task<UpdateSettings?> GetUpdateSettingAsync(
        UpdateSettingsService.UpdateSettingsServiceClient updateSettingsClient,
        UpdateSource updateSource,
        Update update,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Cached
        var updateSetting = await updateSettingsClient.GetBySourceAndTypeAsync(
            new SourceAndTypeRequest { UpdateSource = (int)updateSource, UpdateType = (int)update.Type }, 
            cancellationToken: cancellationToken);

        if (updateSetting is not null)
            return new UpdateSettings()
            {
                Id = updateSetting.Setting.Id,
                UpdateSource = (UpdateSource)updateSetting.Setting.UpdateSource,
                UpdateType = (UpdateType)updateSetting.Setting.UpdateType,
            };
        
        logger.LogWarning($"No enabled UpdateSettings found for Source [{updateSource}] and Type [{update.Type}]");
        return null;
    }
    
    private static async Task<List<long>> GetActiveModulesAsync(
        TgBotModuleService.TgBotModuleServiceClient botModuleClient,
        long updateSettingId,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var activeModules = await botModuleClient
            .GetByUpdateSettingsIdAsync(
                new UpdateSettingsIdRequest 
                { 
                    UpdateSettingsId = updateSettingId 
                }, cancellationToken: cancellationToken);
        
        if (activeModules.Modules.Count == 0)
        {
            logger.LogInformation($"No active ProcessingModule found for update setting ID [{updateSettingId}]");
        }
        
        return activeModules.Modules.ToList();
    }
    
    private static async Task ProduceKafkaMessage(Update update,
        UpdateSettings updateSetting,
        UpdateSource updateSource,
        List<long> activeModuleIds,
        KafkaProducer kafkaProducer,
        string topic,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        
        writer.WriteString("UpdateType", update.Type.ToString());
        writer.WriteString("UpdateSource", updateSource.ToString());
        writer.WriteString("Timestamp", DateTime.UtcNow.ToString("o"));
    
        writer.WritePropertyName("Modules");
        writer.WriteStartArray();
        foreach (var moduleId in activeModuleIds)
        {
            writer.WriteNumberValue(moduleId);
        }
        writer.WriteEndArray();
    
        writer.WritePropertyName("Update");
        JsonSerializer.Serialize(writer, update, JsonSerializerOptions);
        
        writer.WriteEndObject();
        
        await writer.FlushAsync(cancellationToken);
        
        var messageKey = updateSetting.Id.ToString();
        var messageValue = Encoding.UTF8.GetString(stream.ToArray());

        await kafkaProducer.Produce(topic, messageValue, messageKey, cancellationToken);
    }

    
    private static IResult NotifyProcessingResult(
        Update update,
        int activeModuleCount,
        ILogger logger)
    {
        var resultMessage = new StringBuilder()
            .Append("[Update processed] - TYPE[")
            .Append(update.Type)
            .Append("]:[")
            .Append(DateTime.UtcNow)
            .Append("] - found [")
            .Append(activeModuleCount)
            .Append("] active modules")
            .ToString();

        logger.LogInformation(resultMessage);

        return Results.Ok(resultMessage);
    }
}