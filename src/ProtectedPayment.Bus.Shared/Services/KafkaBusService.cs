using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Constants;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Helpers;
using ProtectedPayment.Bus.Shared.Options;
using ProtectedPayment.Bus.Shared.Serialization;
using RabbitMQ.Client;
using System.Text;

namespace ProtectedPayment.Bus.Shared.Services
{
    public sealed class KafkaBusService(ILogger<KafkaBusService> logger, ServiceBusOption busOption) : IBusService
    {
        public static Dictionary<object, string> TopicList = new();

        static KafkaBusService()
        {
            TopicList.Add(typeof(OrderPendingEvent), KafkaBusHelper.GetTopicName<OrderPendingEvent>());
            TopicList.Add(typeof(OrderCancelRequestedEvent), KafkaBusHelper.GetTopicName<OrderCancelRequestedEvent>());
            TopicList.Add(typeof(OrderShippedEvent), KafkaBusHelper.GetTopicName<OrderShippedEvent>());
        }

        public async Task CreateTopics()
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = busOption.KafkaBootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            foreach (var topic in TopicList)
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                if (metadata.Topics.Any(t => t.Topic == topic.Value && t.Error.Code == ErrorCode.NoError))
                {
                    // Topic already exists, skip creation
                    continue;
                }

                var topicSpecification = new TopicSpecification
                {
                    Name = topic.Value,
                    NumPartitions = 3,
                    ReplicationFactor = 1 // 3 => 1 Leader + 2 Replica
                };

                await adminClient.CreateTopicsAsync([topicSpecification]);
            }
        }

        public async Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
        {
            var topicName = TopicList.GetValueOrDefault(message.GetType());

            if (string.IsNullOrEmpty(topicName))
                throw new ArgumentException($"Topic name not found for message type {message.GetType().FullName}");

            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = busOption.KafkaBootstrapServers,
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                EnableIdempotence = false,
                RetryBackoffMs = 2000
            };

            using var producer = new ProducerBuilder<Guid, T>(producerConfig)
                .SetValueSerializer(new ProducerSerializer<T>())
                .SetKeySerializer(new ProducerSerializer<Guid>()).Build();

            var header = new Confluent.Kafka.Headers();

            var kafkaMessage = new Message<Guid, T>
            {
                Value = message,
                Headers = header
            };

            if (headers is not null)
            {
                if (headers.ContainsKey(QueueConstants.MessageKey))
                {
                    kafkaMessage.Key = Guid.Parse(headers[QueueConstants.MessageKey].ToString()!);
                }

                foreach (var kvp in headers)
                {
                    var headerValue = kvp.Value?.ToString() ?? string.Empty;
                    header.Add(kvp.Key, Encoding.UTF8.GetBytes(headerValue));
                }
            }

            var deliveryResult = await producer.ProduceAsync(topicName, kafkaMessage);

            logger.LogInformation(
                $"Message sent to topic {deliveryResult.Topic}, partition {deliveryResult.Partition}, offset {deliveryResult.Offset}");
        }

        public Task<IChannel> CreateChannelAsync() => throw new NotImplementedException();
    }
}
