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

    public static DbContextOptionsBuilder UseInterceptorTriggerEvents(
        this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
    {
        var interceptor = serviceProvider.GetRequiredService<EntityChangeInterceptor>();
        builder.AddInterceptors(interceptor);
        return builder;
    }
}
