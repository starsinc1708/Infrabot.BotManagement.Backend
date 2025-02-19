using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Broker.Kafka;

public class KafkaProducer : IDisposable
{
    private readonly IProducer<string, byte[]> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public KafkaProducer(ILogger<KafkaProducer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var bootstrapServers = configuration["ConnectionStrings:Kafka"]
                               ?? throw new ArgumentNullException();

        _logger.LogInformation("Connecting to Kafka at {BootstrapServers}", bootstrapServers);

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            MessageTimeoutMs = 30000,
            LingerMs = 2,
            BatchSize = 1048576,
            QueueBufferingMaxMessages = 100000,
            EnableIdempotence = true,
            Partitioner = Partitioner.Consistent,
            CompressionType = CompressionType.Snappy,
            MessageSendMaxRetries = 5,
            RetryBackoffMs = 100
        };

        _producer = new ProducerBuilder<string, byte[]>(config)
            .SetLogHandler((_, logMsg) => logger.LogInformation("{KafkaLog}", logMsg.Message))
            .Build();
    }

    public async Task Produce(string topic, object message, string key, CancellationToken ct)
    {
        try
        {
            using var stream = new MemoryStream();
            await using var writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(writer, message, _jsonOptions);
            await writer.FlushAsync(ct);

            var msg = new Message<string, byte[]>
            {
                Key = key,
                Value = stream.ToArray()
            };

            var deliveryReport = await _producer.ProduceAsync(topic, msg, ct);
            _logger.LogInformation("Kafka: Sent to {Topic} [Partition: {Partition}, Offset: {Offset}]",
                deliveryReport.Topic, deliveryReport.Partition, deliveryReport.Offset);
        }
        catch (ProduceException<string, byte[]> ex)
        {
            _logger.LogError(ex, "Kafka: Delivery failed, Reason: {Reason}", ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        Task.Run(() =>
        {
            try
            {
                _producer.Flush(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka: Flush failed");
            }
            finally
            {
                _producer.Dispose();
            }
        });
    }
}
