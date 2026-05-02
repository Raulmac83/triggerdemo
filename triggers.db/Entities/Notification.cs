using System;
using System.Collections.Generic;

namespace triggers.db.Entities;

public partial class Notification
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string? Payload { get; set; }

    public int? UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? TriggerMethod { get; set; }

    public virtual User? User { get; set; }
}
