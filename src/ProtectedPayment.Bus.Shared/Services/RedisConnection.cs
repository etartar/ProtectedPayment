using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Options;
using StackExchange.Redis;

namespace ProtectedPayment.Bus.Shared.Services
{
    internal sealed class RedisConnection : IRedisConnection
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RedisConnection(ServiceBusOption busOption)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(busOption.RedisBootstrapServers);
        }

        public IDatabase GetDatabase(int db = 0)
        {
            return _connectionMultiplexer.GetDatabase(db);
        }

        public ISubscriber GetSubscriber()
        {
            return _connectionMultiplexer.GetSubscriber();
        }
    }
}
