namespace triggers.repo.Notifications;

public interface ITriggerMethodSelector
{
    TriggerMethod Current { get; }
    void Set(TriggerMethod method);
}

public class TriggerMethodSelector : ITriggerMethodSelector
{
    private TriggerMethod _current = TriggerMethod.Interceptor;
    private readonly object _lock = new();

    public TriggerMethod Current
    {
        get { lock (_lock) return _current; }
    }

    public void Set(TriggerMethod method)
    {
        lock (_lock) _current = method;
    }
}
