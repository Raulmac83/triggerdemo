using Audit.Core;
using Audit.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace triggers.events.audit;

/// <summary>
/// Audit.NET data provider that converts EF audit events into <see cref="AuditTriggerEvent"/>
/// and dispatches them to all registered <see cref="IAuditTriggerHandler"/>s in a fresh DI scope.
/// </summary>
public sealed class AuditTriggerDataProvider : AuditDataProvider
{
    private readonly IServiceProvider _rootProvider;

    public AuditTriggerDataProvider(IServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public override object? InsertEvent(AuditEvent auditEvent)
    {
        DispatchAsync(auditEvent, CancellationToken.None).GetAwaiter().GetResult();
        return null;
    }

    public override async Task<object?> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        await DispatchAsync(auditEvent, cancellationToken).ConfigureAwait(false);
        return null;
    }

    public override void ReplaceEvent(object eventId, AuditEvent auditEvent) { }
    public override Task ReplaceEventAsync(object eventId, AuditEvent auditEvent, CancellationToken cancellationToken = default) => Task.CompletedTask;

    private async Task DispatchAsync(AuditEvent auditEvent, CancellationToken ct)
    {
        if (auditEvent is not AuditEventEntityFramework efEvent) return;

        using var scope = _rootProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IAuditTriggerHandler>().ToArray();
        if (handlers.Length == 0) return;

        foreach (var entry in efEvent.EntityFrameworkEvent.Entries)
        {
            var changes = (entry.Changes ?? Enumerable.Empty<EventEntryChange>())
                .GroupBy(c => c.ColumnName)
                .ToDictionary(g => g.Key, g => new AuditColumnChange(g.Last().OriginalValue, g.Last().NewValue));

            var pk = entry.PrimaryKey?.Values?.FirstOrDefault();
            var evt = new AuditTriggerEvent(
                entry.Action,
                entry.Name,
                pk,
                entry.Entity,
                changes);

            foreach (var handler in handlers)
            {
                if (!handler.ShouldHandle(evt)) continue;
                await handler.HandleAsync(evt, ct).ConfigureAwait(false);
            }
        }
    }
}
