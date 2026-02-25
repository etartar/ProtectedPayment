using ProtectedPayment.SharedKernel.Enums;

namespace ProtectedPayment.Domain.Entities;

public class IdempotencyKey
{
    public Guid Key { get; set; }
    public EventType EventType { get; set; }
    public DateTime Created { get; set; }
}
