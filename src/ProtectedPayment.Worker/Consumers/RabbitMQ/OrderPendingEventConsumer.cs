using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderPendingEventConsumer : BaseEventConsumer<OrderPendingEvent>
{
    public OrderPendingEventConsumer(IBusService busService, IServiceProvider serviceProvider) : base(busService, serviceProvider)
    {
        SetHasDeadLetter();
    }
}