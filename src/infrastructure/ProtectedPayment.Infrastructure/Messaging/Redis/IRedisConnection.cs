using StackExchange.Redis;

namespace ProtectedPayment.Infrastructure.Messaging.Redis
{
    public interface IRedisConnection
    {
        IDatabase GetDatabase(int db = 0);
        ISubscriber GetSubscriber();
    }
}
