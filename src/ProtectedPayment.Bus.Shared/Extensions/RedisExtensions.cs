using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Options;
using ProtectedPayment.Bus.Shared.Services;

namespace ProtectedPayment.Bus.Shared.Extensions
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration, bool createStreams = false)
        {
            services.Configure<ServiceBusOption>(configuration.GetSection(nameof(ServiceBusOption)));

            services.AddSingleton<ServiceBusOption>(sp =>
            {
                var optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();

                return optionsServiceBus.Value;
            });

            services.AddSingleton<IRedisConnection, RedisConnection>();

            services.AddSingleton<IBusService, RedisBusService>(sp =>
            {
                var redisConnection = sp.GetRequiredService<IRedisConnection>();

                var logger = sp.GetRequiredService<ILogger<RedisBusService>>();

                var redisBus = new RedisBusService(logger, redisConnection);

                if (createStreams)
                {
                    redisBus.CreateStreams().Wait();
                }

                return redisBus;
            });

            return services;
        }
    }
}
