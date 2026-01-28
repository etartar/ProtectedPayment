using ProtectedPayment.Bus.Shared.Events;

namespace ProtectedPayment.Bus.Shared.Helpers;

public static class KafkaBusHelper
{
    public static string GetTopicName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-topic";
    }

    public static string GetConsumerGroupId<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-consumer-group";
    }
}
