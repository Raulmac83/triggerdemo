using Microsoft.EntityFrameworkCore;
using triggers.db;
using triggers.db.Entities;

namespace triggers.repo.Notifications;

public class NotificationsRepository : INotificationsRepository
{
    private readonly AppDbContext _db;

    public NotificationsRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Notification>> GetRecentAsync(int take, CancellationToken ct = default)
        => await _db.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.Id)
            .Take(take)
            .ToListAsync(ct);

    public async Task<int> ClearAllAsync(CancellationToken ct = default)
    {
        var count = await _db.Notifications.ExecuteDeleteAsync(ct);
        return count;
    }
}
