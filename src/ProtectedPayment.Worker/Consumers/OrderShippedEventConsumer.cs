using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Consumers;

internal sealed class OrderShippedEventConsumer(
    IBusService busService,
    IServiceProvider serviceProvider) : BaseEventConsumer<OrderShippedEvent>(busService, serviceProvider)
{
}
