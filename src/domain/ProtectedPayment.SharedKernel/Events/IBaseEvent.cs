namespace ProtectedPayment.SharedKernel.Events;

public interface IBaseEvent
{
    Guid MessageId { get; }
    DateTime CreatedAt { get; }
}