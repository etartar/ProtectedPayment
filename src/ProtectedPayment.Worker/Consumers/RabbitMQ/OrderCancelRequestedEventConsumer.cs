using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderCancelRequestedEventConsumer(
    IBusService busService,
    IServiceProvider serviceProvider) : BaseEventConsumer<OrderCancelRequestedEvent>(busService, serviceProvider)
{
}
