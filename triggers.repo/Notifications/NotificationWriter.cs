using Microsoft.EntityFrameworkCore;
using triggers.db;
using triggers.db.Entities;

namespace triggers.repo.Notifications;

public class NotificationWriter : INotificationWriter
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public NotificationWriter(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task WriteAsync(NotificationInput input, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        db.Notifications.Add(new Notification
        {
            TriggerMethod = input.TriggerMethod,
            Type          = input.Type,
            Severity      = "Info",
            EntityType    = input.EntityType,
            EntityId      = input.EntityId,
            Title         = input.Title,
            Message       = input.Message,
            Payload       = input.Payload,
            IsRead        = false,
        });
        await db.SaveChangesAsync(ct);
    }
}
