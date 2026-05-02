using System.Text.Json;
using triggers.events.audit;

namespace triggers.repo.Notifications;

public class AuditTriggerHandler : IAuditTriggerHandler
{
    private readonly INotificationWriter _writer;
    private readonly ITriggerMethodSelector _selector;

    public AuditTriggerHandler(INotificationWriter writer, ITriggerMethodSelector selector)
    {
        _writer = writer;
        _selector = selector;
    }

    public bool ShouldHandle(AuditTriggerEvent evt)
        => evt.EntityName.EndsWith(".Trigger", StringComparison.Ordinal) || evt.EntityName == "Trigger";

    public Task HandleAsync(AuditTriggerEvent evt, CancellationToken ct)
    {
        if (_selector.Current != TriggerMethod.AuditNet) return Task.CompletedTask;

        var verb = evt.Action switch
        {
            "Insert" => "created",
            "Update" => "updated",
            "Delete" => "deleted",
            _ => "changed",
        };

        var entityId = evt.PrimaryKey switch
        {
            null => (int?)null,
            int i => i,
            long l => (int?)l,
            _ => int.TryParse(evt.PrimaryKey.ToString(), out var v) ? v : null,
        };

        var name = evt.Entity?.GetType().GetProperty("Name")?.GetValue(evt.Entity)?.ToString() ?? "(unknown)";

        return _writer.WriteAsync(new NotificationInput(
            TriggerMethodNames.AuditNet,
            $"Trigger{evt.Action}",
            "Trigger",
            entityId,
            $"Trigger '{name}' {verb}",
            "Captured by Audit.NET.",
            JsonSerializer.Serialize(new
            {
                action = evt.Action,
                changes = evt.Changes.ToDictionary(kv => kv.Key, kv => new { kv.Value.OldValue, kv.Value.NewValue }),
            })), ct);
    }
}
