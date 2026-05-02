using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using triggers.events.audit;
using triggers.events.domain;
using triggers.events.efcoretriggered;
using triggers.events.interceptor;
using triggers.repo.Auth;
using triggers.repo.Notifications;

namespace triggers.repo;

public static class DependencyInjection
{
    public static IServiceCollection AddTriggersRepo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<ITriggerRepository, TriggerRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INotificationsRepository, NotificationsRepository>();

        services.AddSingleton<ITriggerMethodSelector, TriggerMethodSelector>();
        services.AddScoped<INotificationWriter, NotificationWriter>();

        // Option A — interceptor pipeline
        services.AddInterceptorTriggerEvents()
                .AddEntityChangeHandler<db.Entities.Trigger, InterceptorTriggerHandler>();

        // Option B — EFCoreTriggered pipeline
        var efcoreBuilder = services.AddEfCoreTriggeredEvents();
        efcoreBuilder.AddAfterSaveTrigger<EfCoreTriggeredTriggerHandler>();

        // Option C — domain events
        services.AddDomainEventTriggers()
                .AddDomainEventHandler<TriggerCreatedDomainEvent, DomainTriggerCreatedHandler>()
                .AddDomainEventHandler<TriggerUpdatedDomainEvent, DomainTriggerUpdatedHandler>()
                .AddDomainEventHandler<TriggerDeletedDomainEvent, DomainTriggerDeletedHandler>();

        // Option D — Audit.NET
        services.AddAuditTriggerEvents()
                .AddAuditTriggerHandler<AuditTriggerHandler>();

        return services;
    }
}
