using StackExchange.Redis;

namespace ProtectedPayment.Bus.Shared.Abstracts
{
    public interface IRedisConnection
    {
        IDatabase GetDatabase(int db = 0);
        ISubscriber GetSubscriber();
    }
}
