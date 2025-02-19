using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Broker.Kafka
{
    public class TestConsumerService(ILogger<TestConsumerService> logger, IConfiguration configuration)
        : KafkaBaseConsumerService(logger, configuration, "updates_test")
    {
        protected override async Task HandleMessageAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                var deserializedMessage = JsonSerializer.Deserialize<object>(message);
                await Task.Delay(100, cancellationToken);
                logger.LogInformation("Processed Test message: {Message}", deserializedMessage);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize Kafka message in TestConsumerService");
            }
        }
    }
}