using RabbitMQ.Client;

namespace ProtectedPayment.Bus.Shared.Abstracts
{
    public interface IRabbitMQConnection
    {
        IConnection Connection { get; }
        IChannel Channel { get; }
    }
}
