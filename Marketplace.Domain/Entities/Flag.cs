using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class Flag
{
    public int Id { get; set; }

    public int ReporterId { get; set; }

    public int ReportedId { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Reported { get; set; } = null!;

    public virtual User Reporter { get; set; } = null!;
}
