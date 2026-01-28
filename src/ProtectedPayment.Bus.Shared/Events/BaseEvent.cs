namespace ProtectedPayment.Bus.Shared.Events;

public abstract record BaseEvent(Guid MessageKey) : IBaseEvent
{
    public Guid MessageId => Guid.CreateVersion7();
    public DateTime CreatedAt => DateTime.UtcNow;
}
