namespace triggers.repo.Notifications;

public record NotificationInput(
    string TriggerMethod,
    string Type,
    string EntityType,
    int? EntityId,
    string Title,
    string? Message,
    string? Payload);

public interface INotificationWriter
{
    Task WriteAsync(NotificationInput input, CancellationToken ct);
}
