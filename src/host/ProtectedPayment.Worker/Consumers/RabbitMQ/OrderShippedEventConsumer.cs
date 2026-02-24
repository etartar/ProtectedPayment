using ProtectedPayment.Application.Contracts.Contracts;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.Infrastructure.Messaging.RabbitMQ;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderShippedEventConsumer(
    IRabbitMQConnection rabbitMQConnection,
    IBusService busService,
    IServiceProvider serviceProvider) : BaseEventConsumer<OrderShippedEvent>(rabbitMQConnection, busService, serviceProvider)
{
}
