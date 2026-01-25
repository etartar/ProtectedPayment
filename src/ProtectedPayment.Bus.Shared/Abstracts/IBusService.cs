using ProtectedPayment.Bus.Shared.Events;
using RabbitMQ.Client;

namespace ProtectedPayment.Bus.Shared.Abstracts;

public interface IBusService
{
    Task<IChannel> CreateChannelAsync();
    Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent;
}
