using Evently.Common.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtectedPayment.Database.Database;

namespace ProtectedPayment.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddDbContext<OrderDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        return services;
    }
}
