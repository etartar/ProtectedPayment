using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.Domain.Events;

public record OrderCancelRequestedEvent(Guid OrderId) : BaseEvent(OrderId);
