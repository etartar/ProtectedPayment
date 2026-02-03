using ProtectedPayment.Bus.Shared.Enums;

namespace ProtectedPayment.Database.Entities;

public class IdempotencyKey
{
    public Guid Key { get; set; }
    public EventType EventType { get; set; }
    public DateTime Created { get; set; }
}
