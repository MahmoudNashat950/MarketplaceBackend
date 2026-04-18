using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int Status { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<OrderComment> OrderComments { get; set; } = new List<OrderComment>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
