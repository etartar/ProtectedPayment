namespace ProtectedPayment.Bus.Shared.Events;

public interface IBaseEvent
{
    Guid MessageId { get; }
    DateTime CreatedAt { get; }
}