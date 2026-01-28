namespace ProtectedPayment.Bus.Shared.Events;

public record OrderPendingEvent(Guid OrderId, DateTime ScheduledTime) : BaseEvent(OrderId);