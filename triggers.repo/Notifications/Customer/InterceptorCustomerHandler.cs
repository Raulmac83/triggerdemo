using System.Text.Json;
using triggers.db.Entities;
using triggers.events.interceptor;

namespace triggers.repo.Notifications;

public class InterceptorCustomerHandler : EntityTrigger<Customer>
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public InterceptorCustomerHandler(INotificationWriter writer, ITriggerMethodSelector selector)
    {
        _writer = writer;
        _selector = selector;
    }

    protected override Task OnChangedAsync(
        EntityChangeType kind,
        Customer entity,
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
            Type: $"Customer{kind}",
            EntityType: nameof(Customer),
            EntityId: entity.Id,
            Title: $"Customer '{entity.Name}' {verb}",
            Message: "Captured by SaveChangesInterceptor (via EntityTrigger<Customer>).",
            Payload: JsonSerializer.Serialize(new
            {
                changeType = kind.ToString(),
                modified = modifiedProperties.Select(p => new { p.Name, p.OldValue, p.NewValue }),
            })), cancellationToken);
    }
}
