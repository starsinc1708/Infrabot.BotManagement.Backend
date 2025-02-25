using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace Infrabot.BotManagement.Broker;

public class KafkaMessage
{
    // Тип обновления
    [JsonPropertyName("UpdateType")]
    public required string UpdateType { get; set; }

    // Источник обновления
    [JsonPropertyName("UpdateSource")]
    public required string UpdateSource { get; set; }

    // Временная метка
    [JsonPropertyName("Timestamp")]
    public required string Timestamp { get; set; }

    // Список активных модулей
    [JsonPropertyName("Modules")]
    public required List<long> Modules { get; set; }

    // Данные обновления
    [JsonPropertyName("Update")]
    public required Update Update { get; set; }
}