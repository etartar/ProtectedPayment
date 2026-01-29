using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Bus.Shared.Helpers;

public static class RedisBusHelper
{
    public static string GetStreamName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-stream";
    }

    public static string GetConsumerGroupName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-consumer-group";
    }

    public static string GetConsumerName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-consumer";
    }
}
