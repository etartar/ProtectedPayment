namespace ProtectedPayment.Bus.Shared.Events;

public record OrderConfirmedEvent(Guid OrderId) : BaseEvent(OrderId);
