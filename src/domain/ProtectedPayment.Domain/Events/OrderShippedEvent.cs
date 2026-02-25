using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.Domain.Events;

public record OrderShippedEvent(Guid OrderId) : BaseEvent(OrderId);
