using ProtectedPayment.SharedKernel.Events;

namespace ProtectedPayment.Domain.Events;

public record OrderPendingEvent(Guid OrderId, DateTime ScheduledTime) : BaseEvent(OrderId);