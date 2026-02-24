using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.Domain.Events;

public record OrderConfirmedEvent(Guid OrderId) : BaseEvent(OrderId);
