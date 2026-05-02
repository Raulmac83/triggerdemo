namespace triggers.events.audit;

/// <summary>
/// Normalized representation of one entity change captured by Audit.NET.
/// </summary>
public sealed record AuditTriggerEvent(
    string Action,                // "Insert" | "Update" | "Delete"
    string EntityName,            // CLR type name of the changed entity
    object? PrimaryKey,           // first PK column value if available
    object? Entity,               // the entity instance (post-save state)
    IReadOnlyDictionary<string, AuditColumnChange> Changes);

public sealed record AuditColumnChange(object? OldValue, object? NewValue);
