using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Entities;

namespace WarehouseAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            e.Property(x => x.SKU).IsRequired().HasMaxLength(100);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
            e.Property(x => x.Unit).IsRequired().HasMaxLength(50);
            e.Property(x => x.CompanyId).IsRequired().HasMaxLength(100);
        });

        // Warehouse
        modelBuilder.Entity<Warehouse>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Location).IsRequired().HasMaxLength(200);
            e.Property(x => x.CompanyId).IsRequired().HasMaxLength(100);
        });

        // StockTransaction
        modelBuilder.Entity<StockTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CompanyId).IsRequired().HasMaxLength(100);
            e.HasOne(x => x.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Warehouse)
                .WithMany(w => w.StockTransactions)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
