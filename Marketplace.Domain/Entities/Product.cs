using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public int DeliveryTimeInDays { get; set; }

    public int? UserId { get; set; }

    public decimal? Discount { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Rating> RatingProductId1Navigations { get; set; } = new List<Rating>();

    public virtual ICollection<Rating> RatingProducts { get; set; } = new List<Rating>();

    public virtual User Seller { get; set; } = null!;

    public virtual User? User { get; set; }
}
