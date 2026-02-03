using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderCancelRequestedEventConsumer(
    IRabbitMQConnection rabbitMQConnection,
    IBusService busService,
    IServiceProvider serviceProvider) : BaseEventConsumer<OrderCancelRequestedEvent>(rabbitMQConnection, busService, serviceProvider)
{
}
