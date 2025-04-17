using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FashionShop.Models.Entities
{
    public class FashionShopContext : DbContext
    {
        public FashionShopContext(DbContextOptions<FashionShopContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Color> Color { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Order_item> Order_item { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Wishlist> Wishlist { get; set; }
        public DbSet<Blog> Blog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.category_id);

            modelBuilder.Entity<Color>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Colors)
                .HasForeignKey(c => c.product_id);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.user_id);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.payment_id);

            modelBuilder.Entity<Order_item>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Order_item)
                .HasForeignKey(oi => oi.order_id);

            modelBuilder.Entity<Order_item>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.product_id);

            modelBuilder.Entity<Order_item>()
                .HasOne(oi => oi.Color)
                .WithMany(c => c.OrderItems)
                .HasForeignKey(oi => oi.color_id);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.user_id);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.product_id);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Color)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.color_id);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.user_id);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.product_id);
        }
    }
}