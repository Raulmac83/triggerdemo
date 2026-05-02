namespace triggers.events.interceptor;

public interface IEntityChangeHandler<TEntity> where TEntity : class
{
    Task HandleAsync(EntityChange<TEntity> change, CancellationToken ct);
}
