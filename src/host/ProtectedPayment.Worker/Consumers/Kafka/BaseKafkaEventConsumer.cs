using Confluent.Kafka;
using Newtonsoft.Json;
using ProtectedPayment.Application.Contracts.InboxMessages;
using ProtectedPayment.Domain.Entities;
using ProtectedPayment.Domain.Repositories;
using ProtectedPayment.Infrastructure.Messaging.Kafka.Serialization;
using ProtectedPayment.SharedKernel.Constants;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Events;
using ProtectedPayment.SharedKernel.Helpers;
using ProtectedPayment.SharedKernel.Options;
using ProtectedPayment.SharedKernel.Serialization;
using System.Text;

namespace ProtectedPayment.Worker.Consumers.Kafka
{
    internal abstract class BaseKafkaEventConsumer<TEvent> : BackgroundService where TEvent : BaseEvent
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusOption _busOption;
        protected readonly ILogger<BaseKafkaEventConsumer<TEvent>> _logger;
        protected IConsumer<Guid, TEvent> _consumer;

        protected BaseKafkaEventConsumer(
            IServiceProvider serviceProvider,
            ServiceBusOption busOption,
            ILogger<BaseKafkaEventConsumer<TEvent>> logger)
        {
            _serviceProvider = serviceProvider;
            _busOption = busOption;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _busOption.KafkaBootstrapServers,
                GroupId = KafkaBusHelper.GetConsumerGroupId<TEvent>(),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<Guid, TEvent>(consumerConfig)
                .SetKeyDeserializer(new ConsumerDeserializer<Guid>())
                .SetValueDeserializer(new ConsumerDeserializer<TEvent>())
                .Build();

            _consumer.Subscribe(KafkaBusHelper.GetTopicName<TEvent>());

            await base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Dispose();
            return base.StopAsync(cancellationToken);
        }

        protected async Task ProcessEvent(TEvent @event, Guid idempotencyKey, EventType eventType, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var inboxMessageService = scope.ServiceProvider.GetRequiredService<IInboxMessageService>();

            bool hasIdempotency = await inboxMessageService.HasIdempotencyKey(idempotencyKey, eventType, cancellationToken);

            if (!hasIdempotency)
            {
                string eventData = JsonConvert.SerializeObject(@event, SerializerSettings.Instance);

                await inboxMessageService.SaveInboxMessage(idempotencyKey, eventType, eventData, cancellationToken);
            }
        }

        protected Guid GetIdempotencyKey(ConsumeResult<Guid, TEvent> consumeResult)
        {
            if (!consumeResult.Message.Headers.TryGetLastBytes(QueueConstants.IdempotencyKey, out byte[] idempotencyKeyByte))
            {
                throw new Exception("Idempotency key is missing in message headers.");
            }

            string idempotencyKeyString = Encoding.UTF8.GetString(idempotencyKeyByte);

            if (!Guid.TryParse(idempotencyKeyString, out Guid idempotencyKey))
                throw new Exception("Idempotency key is not a valid GUID.");

            return idempotencyKey;
        }

        protected EventType GetEventType(ConsumeResult<Guid, TEvent> consumeResult)
        {
            if (!consumeResult.Message.Headers.TryGetLastBytes(QueueConstants.EventType, out byte[] eventTypeByte))
            {
                throw new Exception("Event Type is missing in message headers.");
            }

            string eventTypeString = Encoding.UTF8.GetString(eventTypeByte);

            if (!Enum.TryParse<EventType>(eventTypeString, true, out var eventType))
                throw new Exception("Event Type key is not a valid EventType enum.");

            return eventType;
        }
    }
}
