using Microsoft.Extensions.DependencyInjection;
using ProtectedPayment.Application.Contracts.InboxMessages;
using ProtectedPayment.Application.Contracts.Orders;
using ProtectedPayment.Application.Contracts.OutboxMessages;
using ProtectedPayment.Application.InboxMessages;
using ProtectedPayment.Application.Orders;
using ProtectedPayment.Application.OutboxMessages;

namespace ProtectedPayment.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOutboxMessageService, OutboxMessageService>();
        services.AddScoped<IInboxMessageService, InboxMessageService>();

        return services;
    }
}
