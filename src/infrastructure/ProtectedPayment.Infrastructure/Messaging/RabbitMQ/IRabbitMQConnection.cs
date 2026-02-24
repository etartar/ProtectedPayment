using RabbitMQ.Client;

namespace ProtectedPayment.Infrastructure.Messaging.RabbitMQ
{
    public interface IRabbitMQConnection
    {
        IConnection Connection { get; }
        IChannel Channel { get; }
    }
}
