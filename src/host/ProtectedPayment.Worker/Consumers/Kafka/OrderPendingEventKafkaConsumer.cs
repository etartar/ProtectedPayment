using Confluent.Kafka;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Options;
using ProtectedPayment.Worker.Services;

namespace ProtectedPayment.Worker.Consumers.Kafka;

internal sealed class OrderPendingEventKafkaConsumer : BaseKafkaEventConsumer<OrderPendingEvent>
{
    private readonly KafkaService _kafkaStateService;

    public OrderPendingEventKafkaConsumer(
        IServiceProvider serviceProvider,
        ServiceBusOption busOption,
        ILogger<BaseKafkaEventConsumer<OrderPendingEvent>> logger,
        KafkaService kafkaStateService) : base(serviceProvider, busOption, logger)
    {
        _kafkaStateService = kafkaStateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<Guid, OrderPendingEvent>? consumeResult;

            try
            {
                consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

                if (consumeResult is not null)
                {
                    bool commitMessage = await HandleMessageAsync(consumeResult, stoppingToken);

                    if (commitMessage)
                    {
                        // Mesaj işlendi, offset'i commit et
                        _consumer.Commit(consumeResult);
                    }
                }

                // Bekleyen mesajları kontrol et (zamanı geldiyse işle)
                await ProcessDueMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error consuming message: {ex.Message}");

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        await Task.Delay(1000);
    }

    private async Task<bool> HandleMessageAsync(ConsumeResult<Guid, OrderPendingEvent> consumeResult, CancellationToken cancellationToken)
    {
        bool commitMessage = false;

        var now = DateTime.UtcNow;

        var orderPendingEvent = consumeResult.Message.Value!;

        Guid idempotencyKey = GetIdempotencyKey(consumeResult);

        EventType eventType = GetEventType(consumeResult);

        // Zaman kontrolü
        if (orderPendingEvent.ScheduledTime <= now)
        {
            // Zamanı gelmiş, hemen işle
            _logger.LogInformation("Processing payment immediately for Order {OrderId}", orderPendingEvent.OrderId);

            await ProcessEvent(orderPendingEvent, idempotencyKey, eventType, cancellationToken);

            commitMessage = true;
        }
        else
        {
            // Henüz zamanı gelmedi, beklet
            _logger.LogInformation(
                "Payment scheduled for Order {OrderId}, will process at {ScheduledTime} (in {Minutes} minutes)",
                orderPendingEvent.OrderId,
                orderPendingEvent.ScheduledTime,
                (orderPendingEvent.ScheduledTime - now).TotalMinutes
            );

            // Pending listesine ekle (henüz commit etme!)
            _kafkaStateService.AddOrderPendingMessage(
                orderPendingEvent.OrderId,
                new OrderPendingHistory(orderPendingEvent, consumeResult.TopicPartitionOffset, idempotencyKey, eventType));
        }

        return commitMessage;
    }

    private async Task ProcessDueMessagesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var duePayments = _kafkaStateService.PendingMessages
            .Where(kvp => kvp.Value.Event.ScheduledTime <= now)
            .ToList();

        foreach (var (orderId, orderPendingHistory) in duePayments)
        {
            try
            {
                _logger.LogInformation("Processing scheduled payment for Order {OrderId}", orderId);

                await ProcessEvent(orderPendingHistory.Event, orderPendingHistory.IdempotencyKey, orderPendingHistory.EventType, cancellationToken);

                // Commit yap
                _consumer.Commit(new[] { orderPendingHistory.Offset });

                // Listeden çıkar
                _kafkaStateService.RemoveOrderPendingMessage(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for Order {OrderId}", orderId);
            }
        }
    }
}
