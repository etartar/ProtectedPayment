using Confluent.Kafka;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Options;
using ProtectedPayment.Worker.Services;

namespace ProtectedPayment.Worker.Consumers.Kafka;

internal sealed class OrderCancelRequestedEventKafkaConsumer : BaseKafkaEventConsumer<OrderCancelRequestedEvent>
{
    private readonly KafkaStateService _kafkaStateService;

    public OrderCancelRequestedEventKafkaConsumer(
        IServiceProvider serviceProvider,
        ServiceBusOption busOption,
        ILogger<BaseKafkaEventConsumer<OrderCancelRequestedEvent>> logger,
        KafkaStateService kafkaStateService) : base(serviceProvider, busOption, logger)
    {
        _kafkaStateService = kafkaStateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<Guid, OrderCancelRequestedEvent>? consumeResult;

            try
            {
                consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

                if (consumeResult is null || consumeResult.Message.Value is null)
                {
                    continue;
                }

                Guid idempotencyKey = GetIdempotencyKey(consumeResult);

                EventType eventType = GetEventType(consumeResult);

                var orderCancelRequestedEvent = consumeResult.Message.Value;

                _logger.LogInformation("Cancelling payment for Order {OrderId}", orderCancelRequestedEvent!.OrderId);

                // Pending listesinden çıkar
                _kafkaStateService.RemoveOrderPendingMessage(orderCancelRequestedEvent!.OrderId);

                await ProcessEvent(consumeResult.Message.Value, idempotencyKey, eventType);

                _consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error consuming message: {ex.Message}");
            }
        }

        await Task.Delay(1000);
    }
}
