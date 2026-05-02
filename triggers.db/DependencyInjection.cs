using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using triggers.events.audit;
using triggers.events.domain;
using triggers.events.efcoretriggered;
using triggers.events.interceptor;

namespace triggers.db;

public static class DependencyInjection
{
    public static IServiceCollection AddTriggersDb(this IServiceCollection services, string connectionString)
    {
        // Register the factory as the single source of options. Lifetime: Singleton.
        // It owns DbContextOptions<AppDbContext> (also Singleton), with all four trigger
        // pipelines and the Audit.NET interceptor wired in.
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.UseInterceptorTriggerEvents(sp);
            options.UseEfCoreTriggeredEvents(sp);
            options.UseDomainEventTriggers(sp);
            options.UseAuditInterceptor();
        });

        // Provide a scoped AppDbContext for normal consumers (controllers, repos)
        // by delegating to the factory. This keeps a single options registration
        // and avoids the singleton-consumes-scoped DI validation error.
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        return services;
    }
}
