using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class Comment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
