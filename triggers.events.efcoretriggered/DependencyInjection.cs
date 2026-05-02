using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.efcoretriggered;

/// <summary>
/// A typed builder you fill at startup with the trigger types you want registered.
/// </summary>
public sealed class EfCoreTriggeredBuilder
{
    private readonly IServiceCollection _services;

    public EfCoreTriggeredBuilder(IServiceCollection services)
    {
        _services = services;
    }

    internal List<Type> TriggerTypes { get; } = new();

    public EfCoreTriggeredBuilder AddAfterSaveTrigger<TTrigger>() where TTrigger : class
    {
        TriggerTypes.Add(typeof(TTrigger));
        _services.AddScoped<TTrigger>();
        return this;
    }
}

public static class DependencyInjection
{
    public const string MethodName = "EFCoreTriggered";

    /// <summary>
    /// Registers a singleton <see cref="EfCoreTriggeredBuilder"/> so that the consumer can declare
    /// trigger types at startup. The actual <c>UseTriggers(...)</c> call happens in
    /// <see cref="UseEfCoreTriggeredEvents"/> using the collected types.
    /// </summary>
    public static EfCoreTriggeredBuilder AddEfCoreTriggeredEvents(this IServiceCollection services)
    {
        var builder = new EfCoreTriggeredBuilder(services);
        services.AddSingleton(builder);
        return builder;
    }

    /// <summary>
    /// Wires the EntityFrameworkCore.Triggered pipeline into your DbContext options. Call inside
    /// <c>AddDbContext((sp, opts) =&gt; opts.UseEfCoreTriggeredEvents(sp))</c>.
    /// </summary>
    public static DbContextOptionsBuilder UseEfCoreTriggeredEvents(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
        var triggers = serviceProvider.GetService<EfCoreTriggeredBuilder>();
        return builder.UseTriggers(opts =>
        {
            if (triggers is null) return;
            var method = opts.GetType().GetMethods()
                .FirstOrDefault(m => m.Name == "AddTrigger" && m.IsGenericMethod && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 0);
            if (method is null) return;
            foreach (var t in triggers.TriggerTypes)
            {
                method.MakeGenericMethod(t).Invoke(opts, null);
            }
        });
    }
}
