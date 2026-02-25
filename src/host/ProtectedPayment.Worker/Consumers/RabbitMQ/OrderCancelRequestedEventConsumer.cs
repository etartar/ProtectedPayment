using ProtectedPayment.Application.Contracts.Contracts;
using ProtectedPayment.Domain.Events;
using ProtectedPayment.Infrastructure.Messaging.RabbitMQ;

namespace ProtectedPayment.Worker.Consumers.RabbitMQ;

internal sealed class OrderCancelRequestedEventConsumer(
    IRabbitMQConnection rabbitMQConnection,
    IBusService busService,
    IServiceProvider serviceProvider) : BaseEventConsumer<OrderCancelRequestedEvent>(rabbitMQConnection, busService, serviceProvider)
{
}
