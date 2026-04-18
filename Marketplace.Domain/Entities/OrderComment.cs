using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class OrderComment
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int OrderId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
