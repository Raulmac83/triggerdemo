using System.Text.Json;
using triggers.db.Entities;
using triggers.events.interceptor;

namespace triggers.repo.Notifications;

/// <summary>
/// Demonstrates the multi-entity flavour of the new <see cref="EntityTrigger{T1,T2}"/> base
/// class. Fires once for any change to a Product or Customer. Active when the Interceptor
/// method is selected; the per-entity handlers also fire — both notifications will appear,
/// distinguished by Title prefix.
/// </summary>
public class MultiEntityWatchTrigger : EntityTrigger<Product, Customer>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public MultiEntityWatchTrigger(INotificationWriter writer, ITriggerMethodSelector selector)
    {
        _writer = writer;
        _selector = selector;
    }

    protected override Task OnChangedAsync(
        EntityChangeType kind,
        object entity,
        IReadOnlyList<PropertyChange> modifiedProperties,
        CancellationToken cancellationToken)
    {
        if (_selector.Current != TriggerMethod.Interceptor) return Task.CompletedTask;

        var (entityType, entityId, entityName) = entity switch
        {
            Product p  => (nameof(Product),  (int?)p.Id, p.Name),
            Customer c => (nameof(Customer), (int?)c.Id, c.Name),
            _          => (entity.GetType().Name, (int?)null, "(unknown)"),
        };

        var verb = kind switch
        {
            EntityChangeType.Added    => "created",
            EntityChangeType.Modified => "updated",
            EntityChangeType.Deleted  => "deleted",
            _ => "changed",
        };

        return _writer.WriteAsync(new NotificationInput(
            TriggerMethod: TriggerMethodNames.Interceptor,
            Type: $"MultiWatch{kind}",
            EntityType: entityType,
            EntityId: entityId,
            Title: $"[Multi-watch] {entityType} '{entityName}' {verb}",
            Message: "One trigger watching both Product and Customer (EntityTrigger<Product, Customer>).",
            Payload: JsonSerializer.Serialize(new
            {
                entityType,
                changeType = kind.ToString(),
                modified = modifiedProperties.Select(p => new { p.Name, p.OldValue, p.NewValue }),
            })), cancellationToken);
    }
}
