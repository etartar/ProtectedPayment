using Confluent.Kafka;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Options;

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

                await ProcessEvent(consumeResult.Message.Value, idempotencyKey, eventType, stoppingToken);

                _consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error consuming message: {ex.Message}");

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        await Task.Delay(1000);
    }
}
