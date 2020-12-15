using System;
using System.Collections.Generic;
using System.Text;
using COMP2084BeerStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace COMP2084BeerStore.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        // define our model classes so our controllers can access the models
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }

        // override the model creating method
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // define the relationships
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_Products_CategoryId");

            builder.Entity<OrderDetail>()
                .HasOne(p => p.Product)
                .WithMany(c => c.OrderDetails)
                .HasForeignKey(p => p.ProductId)
                .HasConstraintName("FK_OrderDetails_ProductId");

            builder.Entity<Cart>()
                .HasOne(p => p.Product)
                .WithMany(c => c.Carts)
                .HasForeignKey(p => p.ProductId)
                .HasConstraintName("FK_Carts_ProductId");

            builder.Entity<OrderDetail>()
                .HasOne(p => p.Order)
                .WithMany(c => c.OrderDetails)
                .HasForeignKey(p => p.OrderId)
                .HasConstraintName("FK_OrderDetails_OrderId");
        }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
