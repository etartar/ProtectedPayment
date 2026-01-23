using Microsoft.EntityFrameworkCore;
using ProtectedPayment.Database.Database;

namespace ProtectedPayment.Order.API.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using OrderDbContext context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        context.Database.Migrate();
    }
}
