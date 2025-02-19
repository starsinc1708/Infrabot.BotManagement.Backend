using Grpc.Core;
using Infrabot.BotManagement.Domain.Grpc;
using Infrabot.BotManagement.Domain.Repositories;

namespace Infrabot.BotManagement.WebAPI.GrpcServices;

public class ModuleUpdateSettingsServiceImpl(ModuleUpdateSettingsRepository repository)
    : ModuleUpdateSettingsService.ModuleUpdateSettingsServiceBase
{
    public override async Task<ModuleUpdateSettingsResponse> GetByModuleId(ModuleIdRequest request, ServerCallContext context)
    {
        var settings = await repository.GetByModuleIdAsync(request.ModuleId, context.CancellationToken);
        return new ModuleUpdateSettingsResponse
        {
            Settings =
            {
                settings.Select(s => new ModuleUpdateSettings
                {
                    ModuleId = s.ModuleId, 
                    UpdateSettingsId = s.UpdateSettingsId
                })
            }
        };
    }
}