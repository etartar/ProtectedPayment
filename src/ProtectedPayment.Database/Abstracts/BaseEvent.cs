namespace ProtectedPayment.Database.Abstracts;

public abstract class BaseEvent : IBaseEvent
{
    protected BaseEvent()
    {
        MessageId = Guid.CreateVersion7();
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEvent(Guid messageId, DateTime createdAt)
    {
        MessageId = messageId;
        CreatedAt = createdAt;
    }

    public Guid MessageId { get; init; }
    public DateTime CreatedAt { get; init; }
}
