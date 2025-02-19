namespace Infrabot.BotManagement.Domain.DTOs
{
    public abstract class BotModuleDto
    {
        public record ModuleSettingsOperationDto
        {
            public required IEnumerable<long> UpdateSettingIds { get; set; }
        }
        
        public record ModuleBotOperationDto
        {
            public required List<long> ModuleIds { get; set; }
        }

        public record CreateProcessingModuleDto
        {
            public required string Name { get; set; }
            public required string IpAddress { get; set; }
            public int Port { get; set; }
            public required string HealthCheckEndpoint { get; set; }
        }
    }
}