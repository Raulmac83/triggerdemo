using System.Text.Json;
using triggers.db.Entities;
using triggers.events.interceptor;

namespace triggers.repo.Notifications;

public class InterceptorTriggerHandler : IEntityChangeHandler<Trigger>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public InterceptorTriggerHandler(INotificationWriter writer, ITriggerMethodSelector selector)
    {
        _writer = writer;
        _selector = selector;
    }

    public Task HandleAsync(EntityChange<Trigger> change, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.Interceptor) return Task.CompletedTask;

        var verb = change.Type switch
        {
            EntityChangeType.Added    => "created",
            EntityChangeType.Modified => "updated",
            EntityChangeType.Deleted  => "deleted",
            _ => "changed",
        };

        return _writer.WriteAsync(new NotificationInput(
            TriggerMethod: TriggerMethodNames.Interceptor,
            Type: $"Trigger{change.Type}",
            EntityType: nameof(Trigger),
            EntityId: change.Entity.Id,
            Title: $"Trigger '{change.Entity.Name}' {verb}",
            Message: $"Captured by SaveChangesInterceptor.",
            Payload: JsonSerializer.Serialize(new
            {
                changeType = change.Type.ToString(),
                modified = change.ModifiedProperties.Select(p => new { p.Name, p.OldValue, p.NewValue }),
            })), ct);
    }
}
