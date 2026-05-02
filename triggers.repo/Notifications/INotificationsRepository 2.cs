using triggers.db.Entities;

namespace triggers.repo.Notifications;

public interface INotificationsRepository
{
    Task<IReadOnlyList<Notification>> GetRecentAsync(int take, CancellationToken ct = default);
    Task<int> ClearAllAsync(CancellationToken ct = default);
}
