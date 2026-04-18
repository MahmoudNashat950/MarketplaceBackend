using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Domain.Entities;

public partial class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext()
    {
    }

    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Flag> Flags { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderComment> OrderComments { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // OnConfiguring removed - configuration should be provided by the application (DbContextOptions).

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.UserId }, "IX_Comments_ProductId_UserId").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_Comments_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Flag>(entity =>
        {
            entity.HasIndex(e => e.ReportedId, "IX_Flags_ReportedId");

            entity.HasIndex(e => e.ReporterId, "IX_Flags_ReporterId");

            entity.HasOne(d => d.Reported).WithMany(p => p.FlagReporteds)
                .HasForeignKey(d => d.ReportedId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Reporter).WithMany(p => p.FlagReporters)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.BuyerId, "IX_Orders_BuyerId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Buyer).WithMany(p => p.Orders).HasForeignKey(d => d.BuyerId);
        });

        modelBuilder.Entity<OrderComment>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderComments_OrderId");

            entity.HasIndex(e => e.UserId, "IX_OrderComments_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderComments).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.User).WithMany(p => p.OrderComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_OrderItems_ProductId");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.SellerId, "IX_Products_SellerId");

            entity.HasIndex(e => e.UserId, "IX_Products_UserId");

            entity.Property(e => e.Discount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Seller).WithMany(p => p.ProductSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.ProductUsers).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasIndex(e => e.ProductId1, "IX_Ratings_ProductId1");

            entity.HasIndex(e => new { e.ProductId, e.UserId }, "IX_Ratings_ProductId_UserId").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_Ratings_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.RatingProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ProductId1Navigation).WithMany(p => p.RatingProductId1Navigations).HasForeignKey(d => d.ProductId1);

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
