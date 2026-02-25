using Newtonsoft.Json;
using ProtectedPayment.Application.Contracts.Contracts;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.SharedKernel.Events;
using ProtectedPayment.SharedKernel.Helpers;
using ProtectedPayment.SharedKernel.Serialization;
using RabbitMQ.Client;
using System.Text;

namespace ProtectedPayment.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMQBusService(IRabbitMQConnection rabbitMQConnection) : IBusService
{
    public static Dictionary<object, string> ExchangeList = new();

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
        var channel = await rabbitMQConnection.Connection!.CreateChannelAsync();
        foreach (var exchange in ExchangeList)
            await channel.ExchangeDeclareAsync(exchange.Value, ExchangeType.Fanout, true, false);
        await channel.DisposeAsync();
    }

    public async Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
    {
        var exchangeName = ExchangeList.GetValueOrDefault(message.GetType());

        if (string.IsNullOrEmpty(exchangeName))
            throw new ArgumentException($"Exchange name not found for message type {message.GetType().FullName}");

        await rabbitMQConnection.Channel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);

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

        await rabbitMQConnection.Channel.BasicPublishAsync(exchangeName, string.Empty, true, properties, body);
    }
}
