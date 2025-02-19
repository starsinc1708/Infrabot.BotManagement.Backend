using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrabot.BotManagement.Broker.Kafka
{
    public abstract class KafkaBaseConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaBaseConsumerService> _logger;
        private readonly string _topic;

        protected KafkaBaseConsumerService(ILogger<KafkaBaseConsumerService> logger, IConfiguration configuration, string topic)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));

            var bootstrapServers = configuration["ConnectionStrings:Kafka"];
            if (string.IsNullOrEmpty(bootstrapServers))
            {
                _logger.LogError("Kafka bootstrap servers are not configured");
                throw new ArgumentNullException(nameof(bootstrapServers));
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = $"{topic}_consumer_group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetLogHandler((_, logMsg) => logger.LogInformation(logMsg.Message))
                .SetErrorHandler((_, error) => logger.LogError("Kafka Consumer Error: {Reason}", error.Reason))
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);
                        if (consumeResult?.Message == null) continue;

                        _logger.LogInformation("Received message from {Topic}: {Message}", consumeResult.Topic, consumeResult.Message.Value);

                        await HandleMessageAsync(consumeResult.Message.Value, cancellationToken);

                        _consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka Consumer Service ({Topic}) is stopping.", _topic);
            }
            finally
            {
                _consumer.Close();
            }
        }

        protected abstract Task HandleMessageAsync(string message, CancellationToken cancellationToken);

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
