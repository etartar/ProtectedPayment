using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Constants;
using ProtectedPayment.Bus.Shared.Enums;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Helpers;
using ProtectedPayment.Database.Database;
using ProtectedPayment.Database.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal abstract class BaseEventConsumer<TEvent>(
    IRabbitMQConnection rabbitMQConnection,
    IBusService busService,
    IServiceProvider serviceProvider) : BackgroundService where TEvent : BaseEvent
{
    private IChannel? _channel;
    private int _ttl = 600000; // 10 minute TTL
    private string _exchangeName = string.Empty;
    private string _deadLetterExchangeName = string.Empty;
    private string _queueName = string.Empty;
    private string _deadLetterQueueName = string.Empty;
    private string _consumerTag = string.Empty;
    private string _deadLetterConsumerTag = string.Empty;
    private bool _hasDeadLetter = false;

    protected void SetHasDeadLetter()
    {
        _hasDeadLetter = true;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        _channel = await rabbitMQConnection.Connection.CreateChannelAsync();

        _exchangeName = RabbitMQBusHelper.GetExchangeName<TEvent>();
        _deadLetterExchangeName = RabbitMQBusHelper.GetDeadLetterExchangeName<TEvent>();

        _queueName = RabbitMQBusHelper.GetQueueName<TEvent>();
        _deadLetterQueueName = RabbitMQBusHelper.GetDeadLetterQueueName<TEvent>();

        _consumerTag = RabbitMQBusHelper.GetConsumerTag<TEvent>();
        _deadLetterConsumerTag = RabbitMQBusHelper.GetDeadLetterConsumerTag<TEvent>();

        //prefetch count
        //await _channel!.BasicQosAsync(prefetchSize: 0, prefetchCount: 5, global: true, cancellationToken: cancellationToken);

        Dictionary<string, object?> ? args = null;

        if (_hasDeadLetter)
        {
            args = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", _deadLetterExchangeName },
                { "x-message-ttl", _ttl }
            };
        }

        await _channel!.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        await _channel!.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args,
            cancellationToken: cancellationToken);

        await _channel!.QueueBindAsync(
            queue: _queueName,
            exchange: _exchangeName,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        // Declare Dead Letter Exchange and Queue

        if (_hasDeadLetter)
        {
            await _channel!.ExchangeDeclareAsync(
                exchange: _deadLetterExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);

            await _channel!.QueueDeclareAsync(
                queue: _deadLetterQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel!.QueueBindAsync(
                queue: _deadLetterQueueName,
                exchange: _deadLetterExchangeName,
                routingKey: string.Empty,
                cancellationToken: cancellationToken);
        }

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _channel!.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += Consumer_ReceivedAsync;

        string consumerQueue = _hasDeadLetter ? _deadLetterQueueName : _queueName;
        string consumerTag = _hasDeadLetter ? _deadLetterConsumerTag : _consumerTag;

        await _channel!.BasicConsumeAsync(
            queue: consumerQueue,
            autoAck: false,
            consumerTag: consumerTag,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            #region Parse IdempotencyKey

            if (!@event.BasicProperties.Headers!.TryGetValue(QueueConstants.IdempotencyKey, out var getIdempotencyKey))
                //throw new exception with no idempotency key
                throw new Exception("Idempotency key is missing in message headers.");

            if (getIdempotencyKey is not byte[] getIdempotencyKeyBytes)
                throw new Exception("Invalid idempotency key format.");

            string idempotencyKeyString = Encoding.UTF8.GetString(getIdempotencyKeyBytes);

            if (!Guid.TryParse(idempotencyKeyString, out Guid idempotencyKey))
                throw new Exception("Idempotency key is not a valid GUID.");

            #endregion

            #region Parse Event Type

            if (!@event.BasicProperties.Headers!.TryGetValue(QueueConstants.EventType, out var getEventType))
                //throw new exception with no idempotency key
                throw new Exception("Event Type is missing in message headers.");

            if (getEventType is not byte[] getEventTypeBytes)
                throw new Exception("Invalid Event type key format.");

            string eventTypeString = Encoding.UTF8.GetString(getEventTypeBytes);

            if (!Enum.TryParse<EventType>(eventTypeString, true, out var eventType))
                throw new Exception("Event Type key is not a valid EventType enum.");

            #endregion

            string eventData = Encoding.UTF8.GetString(@event.Body.ToArray());

            bool hasIdempotency = await dbContext.IdempotencyKeys.AnyAsync(x => x.Key == idempotencyKey && x.EventType == eventType);

            if (hasIdempotency)
            {
                await _channel!.BasicNackAsync(deliveryTag: @event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            var inbox = new InboxMessage
            {
                Id = Guid.CreateVersion7(),
                Type = $"{eventType}",
                Content = eventData,
                OccurredOnUtc = DateTime.UtcNow
            };

            await dbContext.InboxMessages.AddAsync(inbox);

            await dbContext.IdempotencyKeys.AddAsync(new IdempotencyKey
            {
                Key = idempotencyKey,
                EventType = eventType,
                Created = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            await _channel!.BasicAckAsync(deliveryTag: @event.DeliveryTag, multiple: false);
        }
        catch (Exception)
        {
            await _channel!.BasicRejectAsync(deliveryTag: @event.DeliveryTag, requeue: true);
        }
    }
}