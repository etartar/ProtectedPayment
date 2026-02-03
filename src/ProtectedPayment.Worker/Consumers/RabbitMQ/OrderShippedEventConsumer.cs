using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderShippedEventConsumer(
    IRabbitMQConnection rabbitMQConnection,
    IBusService busService,
    IServiceProvider serviceProvider) : BaseEventConsumer<OrderShippedEvent>(rabbitMQConnection, busService, serviceProvider)
{
}
