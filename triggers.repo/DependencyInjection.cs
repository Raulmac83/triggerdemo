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
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INotificationsRepository, NotificationsRepository>();

        services.AddSingleton<ITriggerMethodSelector, TriggerMethodSelector>();
        services.AddScoped<INotificationWriter, NotificationWriter>();

        // Option A — interceptor pipeline
        services.AddInterceptorTriggerEvents()
                .AddEntityChangeHandler<db.Entities.Trigger, InterceptorTriggerHandler>();
        services.AddEntityTrigger<InterceptorProductHandler>();
        services.AddEntityTrigger<InterceptorCustomerHandler>();
        // Demo: one trigger that fires for any change to Product or Customer
        services.AddEntityTrigger<MultiEntityWatchTrigger>();

        // Option B — EFCoreTriggered pipeline
        var efcoreBuilder = services.AddEfCoreTriggeredEvents();
        efcoreBuilder.AddAfterSaveTrigger<EfCoreTriggeredTriggerHandler>();
        efcoreBuilder.AddEntityTrigger<EFCoreTriggeredProductHandler>();
        efcoreBuilder.AddEntityTrigger<EFCoreTriggeredCustomerHandler>();

        // Option C — domain events
        services.AddDomainEventTriggers()
                .AddDomainEventHandler<TriggerCreatedDomainEvent, DomainTriggerCreatedHandler>()
                .AddDomainEventHandler<TriggerUpdatedDomainEvent, DomainTriggerUpdatedHandler>()
                .AddDomainEventHandler<TriggerDeletedDomainEvent, DomainTriggerDeletedHandler>()
                .AddDomainEventHandler<ProductCreatedDomainEvent, DomainProductCreatedHandler>()
                .AddDomainEventHandler<ProductUpdatedDomainEvent, DomainProductUpdatedHandler>()
                .AddDomainEventHandler<ProductDeletedDomainEvent, DomainProductDeletedHandler>()
                .AddDomainEventHandler<CustomerCreatedDomainEvent, DomainCustomerCreatedHandler>()
                .AddDomainEventHandler<CustomerUpdatedDomainEvent, DomainCustomerUpdatedHandler>()
                .AddDomainEventHandler<CustomerDeletedDomainEvent, DomainCustomerDeletedHandler>();

        // Option D — Audit.NET
        services.AddAuditTriggerEvents()
                .AddAuditTriggerHandler<AuditTriggerHandler>()
                .AddAuditTriggerHandler<AuditProductHandler>()
                .AddAuditTriggerHandler<AuditCustomerHandler>();

        return services;
    }
}
