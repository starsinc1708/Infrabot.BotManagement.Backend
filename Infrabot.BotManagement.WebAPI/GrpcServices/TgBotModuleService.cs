using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Grpc;
using Infrabot.BotManagement.Domain.Repositories;

namespace Infrabot.BotManagement.WebAPI.GrpcServices;

public class TgBotModuleServiceImpl(TgBotModuleRepository repository) : TgBotModuleService.TgBotModuleServiceBase
{
    public override async Task<ProcessingModuleResponse> Add(CreateBotModuleRequest request, ServerCallContext context)
    {
        var module = await repository.Add(new Domain.Models.ProcessingModule
        {
            Name = request.Name,
            IpAddress = request.IpAddress,
            Port = request.Port,
            HealthCheckEndpoint = request.HealthCheckEndpoint
        }, context.CancellationToken);
        
        return module != null ? new ProcessingModuleResponse
        {
            Module = MapModule(module)
        } : new ProcessingModuleResponse { Module = null };
    }

    public override async Task<ProcessingModuleResponse> Update(UpdateBotModuleRequest request, ServerCallContext context)
    {
        var existing = await repository.GetById(request.Id, context.CancellationToken);
        if (existing == null) return new ProcessingModuleResponse();

        existing.Name = request.Name;
        existing.IpAddress = request.IpAddress;
        existing.Port = request.Port;
        existing.HealthCheckEndpoint = request.HealthCheckEndpoint;

        var updated = await repository.Update(existing, context.CancellationToken);
        return updated != null ? new ProcessingModuleResponse { Module = MapModule(updated) } : new ProcessingModuleResponse();
    }

    public override async Task<ProcessingModuleResponse> Patch(PatchBotModuleRequest request, ServerCallContext context)
    {
        var existing = await repository.GetById(request.Id, context.CancellationToken);
        if (existing == null) return new ProcessingModuleResponse();
        
        if (request.HasName && !string.IsNullOrEmpty(request.Name)) 
            existing.Name = request.Name;
        if (request.HasIpAddress && !string.IsNullOrEmpty(request.IpAddress)) 
            existing.IpAddress = request.IpAddress;
        if (request.HasHealthCheckEndpoint && !string.IsNullOrEmpty(request.HealthCheckEndpoint)) 
            existing.HealthCheckEndpoint = request.HealthCheckEndpoint;
        if (request.HasPort) 
            existing.Port = request.Port;

        var updatedBot = await repository.Update(existing, context.CancellationToken);
        return updatedBot != null ? new ProcessingModuleResponse { Module = MapModule(updatedBot) } : new ProcessingModuleResponse();
    }

    public override async Task<DeleteProcessingModuleResponse> Delete(ModuleIdRequest request, ServerCallContext context)
    {
        var existing = await repository.GetById(request.ModuleId, context.CancellationToken);
        if (existing == null) return new DeleteProcessingModuleResponse { Success = false };
        
        await repository.Remove(existing, context.CancellationToken);
        return new DeleteProcessingModuleResponse { Success = true };
    }

    public override async Task<ProcessingModuleListResponse> GetAll(Empty request, ServerCallContext context)
    {
        var modules = await repository.GetAll(context.CancellationToken);
        var response = new ProcessingModuleListResponse();
        response.Modules.AddRange(modules.Select(MapModule));
        return response;
    }

    public override async Task<ProcessingModuleIdListResponse> GetByUpdateSettingsId(UpdateSettingsIdRequest request, ServerCallContext context)
    {
        var modules = await repository.GetByUpdateSettingsIdAsync(request.UpdateSettingsId, context.CancellationToken);
        return new ProcessingModuleIdListResponse { Modules = { modules } };
    }

    public override async Task<ProcessingModuleWithSettingsResponse> GetWithSettings(ModuleIdRequest request, ServerCallContext context)
    {
        var module = await repository.GetWithSettings(request.ModuleId, context.CancellationToken);
        return module == null ? new ProcessingModuleWithSettingsResponse() 
            : CreateResponse(module);
    }

    public override async Task<ProcessingModuleWithSettingsResponse> AddSettings(ModuleSettingsOperationRequest request, ServerCallContext context)
    {
        var module = await repository.AddSettings(
            request.ModuleId,
            new BotModuleDto.ModuleSettingsOperationDto
            {
                UpdateSettingIds = request.UpdateSettingIds
            },
            context.CancellationToken);

        return module == null ? new ProcessingModuleWithSettingsResponse() 
            : CreateResponse(module);
    }

    public override async Task<ProcessingModuleWithSettingsResponse> RemoveSettings(ModuleSettingsOperationRequest request, ServerCallContext context)
    {
        var module = await repository.RemoveSettings(
            request.ModuleId,
            new BotModuleDto.ModuleSettingsOperationDto
            {
                UpdateSettingIds = request.UpdateSettingIds
            },
            context.CancellationToken);

        return module == null ? new ProcessingModuleWithSettingsResponse() 
            : CreateResponse(module);
    }

    private static ProcessingModuleWithSettingsResponse CreateResponse(Domain.Models.ProcessingModule module)
    {
        var response = new ProcessingModuleWithSettingsResponse
        {
            Module = MapModule(module)
        };

        response.UpdateSettings.AddRange(module.UpdateSettings.Select(setting 
            => new ModuleUpdateSettings
            {
                ModuleId = setting.ModuleId,
                UpdateSettingsId = setting.UpdateSettingsId
            }));

        return response;
    }

    private static ProcessingModule MapModule(Domain.Models.ProcessingModule module) => new()
        {
            Id = module.Id,
            Name = module.Name,
            HealthCheckEndpoint = module.HealthCheckEndpoint,
            IpAddress = module.IpAddress,
            Port = module.Port,
        };
}
