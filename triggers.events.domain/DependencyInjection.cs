using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.domain;

public static class DependencyInjection
{
    public const string MethodName = "DomainEvents";

    public static IServiceCollection AddDomainEventTriggers(this IServiceCollection services)
    {
        services.AddSingleton<DomainEventInterceptor>();
        return services;
    }

    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }

    public static DbContextOptionsBuilder UseDomainEventTriggers(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
        var interceptor = serviceProvider.GetRequiredService<DomainEventInterceptor>();
        builder.AddInterceptors(interceptor);
        return builder;
    }
}
