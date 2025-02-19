using System.Text.Json.Serialization;

namespace Infrabot.BotManagement.Domain.DTOs.BotRequests;

public abstract class SetWebhook
{
    public record Request
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public required string Url { get; set; }
        public int? MaxConnections { get; set; }
        public bool DropPendingUpdates { get; set; }
        public string[]? AllowedUpdates { get; set; }
    }
}