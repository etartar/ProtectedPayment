using Newtonsoft.Json;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Helpers;
using ProtectedPayment.Bus.Shared.Options;
using ProtectedPayment.Bus.Shared.Serialization;
using RabbitMQ.Client;
using System.Text;

namespace ProtectedPayment.Bus.Shared.Services;

public sealed class RabbitMQBusService(ServiceBusOption busOption) : IBusService
{
    public static Dictionary<object, string> ExchangeList = new();
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task Init()
    {
        //Factory Method Design method
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(busOption.RabbitMqConnectionString)
        };

        _connection = await connectionFactory.CreateConnectionAsync();

        _channel = await _connection!.CreateChannelAsync(new CreateChannelOptions(true, true));
    }

    static RabbitMQBusService()
    {
        ExchangeList.Add(typeof(OrderPendingEvent), RabbitMQBusHelper.GetExchangeName<OrderPendingEvent>());
        ExchangeList.Add(typeof(OrderCancelRequestedEvent), RabbitMQBusHelper.GetExchangeName<OrderCancelRequestedEvent>());
        ExchangeList.Add(typeof(OrderCancelledEvent), RabbitMQBusHelper.GetExchangeName<OrderCancelledEvent>());
        ExchangeList.Add(typeof(OrderConfirmedEvent), RabbitMQBusHelper.GetExchangeName<OrderConfirmedEvent>());
        ExchangeList.Add(typeof(OrderShippedEvent), RabbitMQBusHelper.GetExchangeName<OrderShippedEvent>());
    }

    public async Task CreateExchanges()
    {
        var channel = await _connection!.CreateChannelAsync();
        foreach (var exchange in ExchangeList)
            await channel.ExchangeDeclareAsync(exchange.Value, ExchangeType.Fanout, true, false);
        await channel.DisposeAsync();
    }

    public Task<IChannel> CreateChannelAsync()
    {
        return _connection!.CreateChannelAsync();
    }

    public async Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
    {
        var exchangeName = ExchangeList.GetValueOrDefault(message.GetType());

        if (string.IsNullOrEmpty(exchangeName))
            throw new ArgumentException($"Exchange name not found for message type {message.GetType().FullName}");

        await _channel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);

        var eventAsJsonData = JsonConvert.SerializeObject(message, SerializerSettings.Instance);

        var body = Encoding.UTF8.GetBytes(eventAsJsonData);

        var properties = new BasicProperties
        {
            Persistent = true
        };

        if (headers is not null)
        {
            properties.Headers = headers!;
        }

        await _channel.BasicPublishAsync(exchangeName, string.Empty, true, properties, body);
    }
}
