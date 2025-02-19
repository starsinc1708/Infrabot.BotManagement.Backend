using Grpc.Core;
using Infrabot.BotManagement.Domain.Enums;
using Infrabot.BotManagement.Domain.Grpc;
using Infrabot.BotManagement.Domain.Repositories;
using Telegram.Bot.Types.Enums;

namespace Infrabot.BotManagement.WebAPI.GrpcServices;

public class UpdateSettingsServiceImpl(UpdateSettingsRepository repository)
    : UpdateSettingsService.UpdateSettingsServiceBase
{
    public override async Task<UpdateSettingsListResponse> GetBySourceId(SourceIdRequest request, ServerCallContext context)
    {
        var settings = await repository.GetBySourceId(request.SourceId, context.CancellationToken);
        return new UpdateSettingsListResponse { Settings = { settings.Select(MapSettings) } };
    }
    
    public override async Task<UpdateSettingsListResponse> GetByTypeId(TypeIdRequest request, ServerCallContext context)
    {
        var settings = await repository.GetByTypeId(request.TypeId, context.CancellationToken);
        return new UpdateSettingsListResponse { Settings = { settings.Select(MapSettings) } };
    }
    
    public override async Task<UpdateSettingsResponse> GetBySourceAndType(SourceAndTypeRequest request, ServerCallContext context)
    {
        var settings = await repository.GetBySourceAndType((UpdateSource)request.UpdateSource, (UpdateType)request.UpdateType, context.CancellationToken);
        return settings == null ? new UpdateSettingsResponse() : new UpdateSettingsResponse
        {
            Setting = MapSettings(settings)
        };
    }
    
    private static UpdateSettings MapSettings(Domain.Models.UpdateSettings settings) => new()
    {
        Id = settings.Id,
        UpdateSource = (int)settings.UpdateSource,
        UpdateType = (int)settings.UpdateType
    };
}