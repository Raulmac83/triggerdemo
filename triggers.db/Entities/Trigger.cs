using System;
using System.Collections.Generic;

namespace triggers.db.Entities;

public partial class Trigger
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }
}
