using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.Domain.Events;

public record OrderCancelledEvent(Guid OrderId) : BaseEvent(OrderId);
