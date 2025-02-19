using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Models;

namespace Infrabot.BotManagement.Domain.Mappings;

public static class ProcessingModuleMapping
{
    public static ProcessingModule MapToModel(this BotModuleDto.CreateProcessingModuleDto dto)
    {
        return new ProcessingModule()
        {
            Name = dto.Name,
            IpAddress = dto.IpAddress,
            Port = dto.Port,
            HealthCheckEndpoint = dto.HealthCheckEndpoint,
        };
    }
}