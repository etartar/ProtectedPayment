using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.SharedKernel.Helpers;

public static class RabbitMQBusHelper
{
    public static string GetExchangeName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-exchange";
    }
    
    public static string GetDeadLetterExchangeName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-deadletter-exchange";
    }

    public static string GetQueueName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-queue";
    }

    public static string GetDeadLetterQueueName<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-deadletter-queue";
    }

    public static string GetConsumerTag<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-consumer.tag";
    }

    public static string GetDeadLetterConsumerTag<T>() where T : BaseEvent
    {
        return $"{typeof(T).Name.ToLower()}-deadletter-consumer.tag";
    }
}
