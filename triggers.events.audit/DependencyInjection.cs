using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.audit;

public static class DependencyInjection
{
    public const string MethodName = "AuditNet";

    /// <summary>
    /// Registers Audit.NET trigger services. Chain <see cref="AddAuditTriggerHandler{THandler}"/> to add handlers.
    /// </summary>
    public static IServiceCollection AddAuditTriggerEvents(this IServiceCollection services)
    {
        services.AddSingleton<AuditTriggerDataProvider>();
        return services;
    }

    public static IServiceCollection AddAuditTriggerHandler<THandler>(this IServiceCollection services)
        where THandler : class, IAuditTriggerHandler
    {
        services.AddScoped<IAuditTriggerHandler, THandler>();
        return services;
    }

    /// <summary>
    /// Activates Audit.NET globally and points it at the trigger data provider. Call once at startup
    /// (e.g. right after <c>app.Services</c> is built).
    /// </summary>
    public static IServiceProvider UseAuditTriggerEvents(this IServiceProvider serviceProvider)
    {
        var provider = serviceProvider.GetRequiredService<AuditTriggerDataProvider>();
        Audit.Core.Configuration.Setup()
            .UseCustomProvider(provider);
        Audit.EntityFramework.Configuration.Setup()
            .ForAnyContext(c => c.IncludeEntityObjects());
        return serviceProvider;
    }

    /// <summary>
    /// Adds the Audit.NET EF interceptor to your DbContext options. The interceptor captures
    /// changes during SaveChanges and feeds them into the configured data provider.
    /// </summary>
    public static DbContextOptionsBuilder UseAuditInterceptor(this DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditSaveChangesInterceptor());
        return builder;
    }
}
