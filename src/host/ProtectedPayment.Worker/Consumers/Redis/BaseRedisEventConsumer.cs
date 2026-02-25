using Newtonsoft.Json;
using ProtectedPayment.Application.Contracts.InboxMessages;
using ProtectedPayment.Infrastructure.Messaging.Redis;
using ProtectedPayment.SharedKernel.Constants;
using ProtectedPayment.SharedKernel.Enums;
using ProtectedPayment.SharedKernel.Events;
using ProtectedPayment.SharedKernel.Helpers;
using ProtectedPayment.SharedKernel.Serialization;
using StackExchange.Redis;

namespace ProtectedPayment.Worker.Consumers.Redis;

internal abstract class BaseRedisEventConsumer<TEvent> : BackgroundService where TEvent : BaseEvent
{
    protected readonly ILogger<BaseRedisEventConsumer<TEvent>> _logger;
    private readonly IRedisConnection _redisConnection;
    private readonly IServiceProvider _serviceProvider;
    protected IDatabase _db;
    protected string _streamName;
    protected string _consumerGroupName;
    protected string _consumerName;

    protected BaseRedisEventConsumer(
        ILogger<BaseRedisEventConsumer<TEvent>> logger,
        IRedisConnection redisConnection,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _redisConnection = redisConnection;
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        _db = _redisConnection.GetDatabase();

        _streamName = RedisBusHelper.GetStreamName<TEvent>();
        _consumerGroupName = RedisBusHelper.GetConsumerGroupName<TEvent>();
        _consumerName = RedisBusHelper.GetConsumerName<TEvent>();

        var groups = await _db.StreamGroupInfoAsync(_streamName);

        if (groups.All(g => g.Name != _consumerGroupName))
        {
            await _db.StreamCreateConsumerGroupAsync(_streamName, _consumerGroupName, StreamPosition.Beginning);

            _logger.LogInformation("Consumer group {ConsumerGroup} created on stream {StreamName}",
                _consumerGroupName, _streamName);
        }
        else
        {
            _logger.LogInformation("Consumer group {ConsumerGroup} already exists on stream {StreamName}",
                _consumerGroupName, _streamName);
        }

        await base.StartAsync(cancellationToken);
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

    protected Guid GetIdempotencyKey(NameValueEntry[] entries)
    {
        string? idempotencyKeyString = entries.Where(x => x.Name == QueueConstants.IdempotencyKey).Select(x => x.Value).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(idempotencyKeyString))
        {
            throw new Exception("Idempotency key is missing in message headers.");
        }

        if (!Guid.TryParse(idempotencyKeyString, out Guid idempotencyKey))
            throw new Exception("Idempotency key is not a valid GUID.");

        return idempotencyKey;
    }

    protected EventType GetEventType(NameValueEntry[] entries)
    {
        string? eventTypeString = entries.Where(x => x.Name == QueueConstants.EventType).Select(x => x.Value).FirstOrDefault();

        if (!Enum.TryParse<EventType>(eventTypeString, true, out var eventType))
            throw new Exception("Event Type key is not a valid EventType enum.");

        return eventType;
    }

    protected TEvent GetEvent(NameValueEntry[] entries)
    {
        string? eventString = entries.Where(x => x.Name == QueueConstants.Event).Select(x => x.Value).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(eventString))
        {
            throw new Exception("Event is missing in message headers.");
        }

        return JsonConvert.DeserializeObject<TEvent>(eventString!)!;
    }
}
