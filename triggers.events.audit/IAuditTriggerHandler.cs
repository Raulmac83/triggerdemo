namespace triggers.events.audit;

public interface IAuditTriggerHandler
{
    /// <summary>
    /// Optional filter — return false to skip this event.
    /// Default: handle all events for the configured entity types.
    /// </summary>
    bool ShouldHandle(AuditTriggerEvent evt) => true;

    Task HandleAsync(AuditTriggerEvent evt, CancellationToken ct);
}
