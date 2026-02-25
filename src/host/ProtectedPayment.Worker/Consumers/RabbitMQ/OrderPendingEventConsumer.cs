using ProtectedPayment.Application.Contracts.Contracts;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.Infrastructure.Messaging.RabbitMQ;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderPendingEventConsumer : BaseEventConsumer<OrderPendingEvent>
{
    public OrderPendingEventConsumer(
        IRabbitMQConnection rabbitMQConnection,
        IBusService busService,
        IServiceProvider serviceProvider) : base(rabbitMQConnection, busService, serviceProvider)
    {
        SetHasDeadLetter();
    }
}