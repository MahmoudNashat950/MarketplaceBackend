using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int Value { get; set; }

    public int? ProductId1 { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Product? ProductId1Navigation { get; set; }

    public virtual User User { get; set; } = null!;
}
