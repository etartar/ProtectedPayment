using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Bus.Shared.Abstracts;

public interface IBusService
{
    Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent;
}
