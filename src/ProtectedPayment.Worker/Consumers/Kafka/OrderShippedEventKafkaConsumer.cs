using Confluent.Kafka;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Options;

namespace ProtectedPayment.Worker.Consumers.Kafka;

internal sealed class OrderShippedEventKafkaConsumer : BaseKafkaEventConsumer<OrderShippedEvent>
{
    public OrderShippedEventKafkaConsumer(
        IServiceProvider serviceProvider,
        ServiceBusOption busOption,
        ILogger<BaseKafkaEventConsumer<OrderShippedEvent>> logger) : base(serviceProvider, busOption, logger)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<Guid, OrderShippedEvent>? consumeResult;

            try
            {
                consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

                if (consumeResult is null || consumeResult.Message.Value is null)
                {
                    continue;
                }

                Guid idempotencyKey = GetIdempotencyKey(consumeResult);

                EventType eventType = GetEventType(consumeResult);

                await ProcessEvent(consumeResult.Message.Value, idempotencyKey, eventType);

                _consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                // 1. options = save partition and offset to a database table for later reprocessing
                // 2. options = move message to error-topic

                _logger.LogCritical(ex, $"Error consuming message: {ex.Message}");
            }
        }

        await Task.Delay(1000);
    }
}
