using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.interceptor;

public static class DependencyInjection
{
    public const string MethodName = "Interceptor";

    public static IServiceCollection AddInterceptorTriggerEvents(this IServiceCollection services)
    {
        services.AddSingleton<EntityChangeInterceptor>();
        return services;
    }

    public static IServiceCollection AddEntityChangeHandler<TEntity, THandler>(this IServiceCollection services)
        where TEntity : class
        where THandler : class, IEntityChangeHandler<TEntity>
    {
        services.AddScoped<IEntityChangeHandler<TEntity>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers a trigger derived from <see cref="EntityTrigger{TEntity}"/> (single entity) or
    /// <see cref="MultiEntityTriggerBase"/> (multi-entity, e.g. <c>EntityTrigger&lt;T1,T2&gt;</c>).
    /// Discovers <see cref="IEntityChangeHandler{T}"/> and <see cref="IAnyEntityChangeHandler"/>
    /// implementations and routes all of them to a single scoped instance.
    /// </summary>
    public static IServiceCollection AddEntityTrigger<TTrigger>(this IServiceCollection services)
        where TTrigger : class
    {
        services.AddScoped<TTrigger>();

        foreach (var iface in typeof(TTrigger).GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityChangeHandler<>))
            {
                services.AddScoped(iface, sp => sp.GetRequiredService<TTrigger>());
            }
        }

        if (typeof(IAnyEntityChangeHandler).IsAssignableFrom(typeof(TTrigger)))
        {
            services.AddScoped<IAnyEntityChangeHandler>(sp => (IAnyEntityChangeHandler)sp.GetRequiredService<TTrigger>());
        }

        return services;
    }

    public static DbContextOptionsBuilder UseInterceptorTriggerEvents(
        this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
    {
        var interceptor = serviceProvider.GetRequiredService<EntityChangeInterceptor>();
        builder.AddInterceptors(interceptor);
        return builder;
    }
}
