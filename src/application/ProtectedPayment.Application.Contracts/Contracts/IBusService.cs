using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.Application.Contracts.Contracts;

public interface IBusService
{
    Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent;
}
