namespace ProtectedPayment.Bus.Shared.Events;

public record OrderCancelledEvent(Guid OrderId) : BaseEvent(OrderId);
