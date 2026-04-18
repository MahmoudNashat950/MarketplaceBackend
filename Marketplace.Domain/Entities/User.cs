using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Flag> FlagReporteds { get; set; } = new List<Flag>();

    public virtual ICollection<Flag> FlagReporters { get; set; } = new List<Flag>();

    public virtual ICollection<OrderComment> OrderComments { get; set; } = new List<OrderComment>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> ProductSellers { get; set; } = new List<Product>();

    public virtual ICollection<Product> ProductUsers { get; set; } = new List<Product>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
