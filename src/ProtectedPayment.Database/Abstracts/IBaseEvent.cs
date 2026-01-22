namespace ProtectedPayment.Database.Abstracts;

public interface IBaseEvent
{
    Guid MessageId { get; }
    DateTime CreatedAt { get; }
}