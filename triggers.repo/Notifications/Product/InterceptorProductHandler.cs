using System.Text.Json;
using triggers.db.Entities;
using triggers.events.interceptor;

namespace triggers.repo.Notifications;

/// <summary>
/// Uses the new <see cref="EntityTrigger{TEntity}"/> base class from triggers.events.interceptor.
/// </summary>
public class InterceptorProductHandler : EntityTrigger<Product>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public InterceptorProductHandler(INotificationWriter writer, ITriggerMethodSelector selector)
    {
        _writer = writer;
        _selector = selector;
    }

    protected override Task OnChangedAsync(
        EntityChangeType kind,
        Product entity,
        IReadOnlyList<PropertyChange> modifiedProperties,
        CancellationToken cancellationToken)
    {
        if (_selector.Current != TriggerMethod.Interceptor) return Task.CompletedTask;

        var verb = kind switch
        {
            EntityChangeType.Added    => "created",
            EntityChangeType.Modified => "updated",
            EntityChangeType.Deleted  => "deleted",
            _ => "changed",
        };

        return _writer.WriteAsync(new NotificationInput(
            TriggerMethod: TriggerMethodNames.Interceptor,
            Type: $"Product{kind}",
            EntityType: nameof(Product),
            EntityId: entity.Id,
            Title: $"Product '{entity.Name}' {verb}",
            Message: "Captured by SaveChangesInterceptor (via EntityTrigger<Product>).",
            Payload: JsonSerializer.Serialize(new
            {
                changeType = kind.ToString(),
                modified = modifiedProperties.Select(p => new { p.Name, p.OldValue, p.NewValue }),
            })), cancellationToken);
    }
}
