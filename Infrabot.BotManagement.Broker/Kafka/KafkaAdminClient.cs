using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Broker.Kafka;

public class KafkaAdminClient : IDisposable
{
    private readonly IAdminClient _adminClient;
    private readonly ILogger<KafkaAdminClient> _logger;

    public KafkaAdminClient(ILogger<KafkaAdminClient> logger, IConfiguration configuration)
    {
        var bootstrapServers = configuration["ConnectionStrings:Kafka"];
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrEmpty(bootstrapServers))
        {
            _logger.LogError("Kafka bootstrap servers are not configured");
            throw new ArgumentNullException(nameof(bootstrapServers));
        }

        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };

        _adminClient = new AdminClientBuilder(config).Build();
    }

    public async Task CreateTopicAsync(string topicName, int partitions, short replicationFactor, CancellationToken ct)
    {
        try
        {
            var topicSpecification = new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitions,
                ReplicationFactor = replicationFactor,
                Configs = new Dictionary<string, string>
                {
                    { "retention.ms", "604800000" }, // 7 дней
                    { "cleanup.policy", "delete" }
                }
            };

            await _adminClient.CreateTopicsAsync([topicSpecification], new CreateTopicsOptions
            {
                RequestTimeout = TimeSpan.FromSeconds(30),
                OperationTimeout = TimeSpan.FromSeconds(30)
            });

            _logger.LogInformation("Topic {TopicName} created successfully", topicName);
        }
        catch (CreateTopicsException ex)
        {
            _logger.LogError(ex, "Failed to create topic {TopicName}: {Reason}", topicName, ex.Results[0].Error.Reason);
            throw;
        }
    }
    
    public bool TopicExistsAsync(string topicName)
    {
        var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(30));
        return metadata.Topics.Any(t => t.Topic == topicName);
    }
    
    public void Dispose()
    {
        _adminClient.Dispose();
    }
}