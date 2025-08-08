using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FUMiniTikiSystem.DAL.DBContext
{
    public class FUMiniTikiSystemDbContext : DbContext
    {
        public FUMiniTikiSystemDbContext()
        {
        }

        public FUMiniTikiSystemDbContext(DbContextOptions<FUMiniTikiSystemDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public static string GetConnectionString(string connectionStringName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = config.GetConnectionString(connectionStringName);
            return connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(od => new { od.ProductID, od.OrderID });
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => new { ci.ProductID, ci.CartID });
            });
        }

        public void ResetIdentitySeed(string tableName)
        {
            Database.ExecuteSqlRaw($"DBCC CHECKIDENT ('{tableName}', RESEED, 0)");
        }

        public void ResetAllIdentitySeeds()
        {
            ResetIdentitySeed("Categories");
            ResetIdentitySeed("Products");
            ResetIdentitySeed("Customers");
            ResetIdentitySeed("Orders");
            ResetIdentitySeed("OrderDetails");
            ResetIdentitySeed("Carts");
            ResetIdentitySeed("CartItems");
            ResetIdentitySeed("Payments");
            ResetIdentitySeed("Reviews");
        }
    }
}
