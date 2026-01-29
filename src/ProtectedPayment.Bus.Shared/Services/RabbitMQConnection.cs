using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Options;
using RabbitMQ.Client;

namespace ProtectedPayment.Bus.Shared.Services
{
    internal sealed class RabbitMQConnection(ServiceBusOption busOption) : IRabbitMQConnection
    {
        private IConnection? _connection;
        private IChannel? _channel;

        public async Task Init()
        {
            //Factory Method Design method
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(busOption.RabbitMqConnectionString)
            };

            _connection = await connectionFactory.CreateConnectionAsync();

            _channel = await _connection!.CreateChannelAsync(new CreateChannelOptions(true, true));
        }

        public IConnection Connection
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_connection, nameof(_connection));

                return _connection;
            }
        }

        public IChannel Channel
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_channel, nameof(_channel));

                return _channel;
            }
        }
    }
}
