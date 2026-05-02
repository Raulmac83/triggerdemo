namespace triggers.repo.Notifications;

public enum TriggerMethod
{
    Interceptor,
    EFCoreTriggered,
    DomainEvents,
    AuditNet,
}

public static class TriggerMethodNames
{
    public const string Interceptor     = "Interceptor";
    public const string EFCoreTriggered = "EFCoreTriggered";
    public const string DomainEvents    = "DomainEvents";
    public const string AuditNet        = "AuditNet";

    public static string ToName(this TriggerMethod m) => m switch
    {
        TriggerMethod.Interceptor     => Interceptor,
        TriggerMethod.EFCoreTriggered => EFCoreTriggered,
        TriggerMethod.DomainEvents    => DomainEvents,
        TriggerMethod.AuditNet        => AuditNet,
        _ => throw new ArgumentOutOfRangeException(nameof(m)),
    };

    public static bool TryParse(string s, out TriggerMethod m)
        => Enum.TryParse(s, ignoreCase: true, out m);
}
