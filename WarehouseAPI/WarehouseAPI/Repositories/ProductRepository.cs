using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.DTOs.Product;
using WarehouseAPI.Entities;
using WarehouseAPI.Repositories.Interfaces;

namespace WarehouseAPI.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    // [KURAL-3] EF Core zorunlu — Raw SQL/Dapper/ADO.NET yasak
    // [KURAL-5] Server-side pagination — Skip/Take backend'de yapılır
    // [KURAL-MT] CompanyId ile filtreleme — başka şirketin verisi görünmez
    // [KURAL-2] Soft delete filtresi — IsDeleted=false olanlar listelenir
    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductListRequestDto request)
    {
        // [KURAL-MT] Her zaman CompanyId ile filtrele + [KURAL-2] IsDeleted=false
        var query = _context.Products
            .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.ProductName.Contains(request.Search) || x.SKU.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(x => x.Category == request.Category);

        // [KURAL-5] Önce toplam sayıyı al, sonra sayfayı getir
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize) // [KURAL-5] Skip
            .Take(request.PageSize)                       // [KURAL-5] Take
            .ToListAsync();

        return (items, totalCount);
    }

    // [KURAL-MT] CompanyId kontrolü — yanlış şirket erişimi engellenir
    // [KURAL-2] IsDeleted=false — silinmiş kayıt döndürülmez
    public async Task<Product?> GetByIdAsync(int id, string companyId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted);
    }

    public async Task<bool> SKUExistsAsync(string sku, string companyId, int? excludeId = null)
    {
        var query = _context.Products
            .Where(x => x.SKU == sku && x.CompanyId == companyId && !x.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    // [KURAL-3] Tüm DB işlemleri EF Core ile — Raw SQL yasak
    // [KURAL-4] EntityState.Modified zorunlu — production'da değişiklikler kaydedilmez
    public async Task UpdateAsync(Product product)
    {
        _context.Entry(product).State = EntityState.Modified; // KURAL-4
        await _context.SaveChangesAsync();
    }

    // [KURAL-2] Soft delete — IsDeleted=true, fiziksel silme yok
    // [KURAL-4] EntityState.Modified zorunlu
    public async Task<bool> SoftDeleteAsync(int id, string companyId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted);

        if (product == null) return false;

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        _context.Entry(product).State = EntityState.Modified; // KURAL-4
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCurrentStockAsync(int productId, string companyId)
    {
        var stockIn = await _context.StockTransactions
            .Where(x => x.ProductId == productId && x.CompanyId == companyId
                     && x.TransactionType == TransactionType.In && !x.IsDeleted)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        var stockOut = await _context.StockTransactions
            .Where(x => x.ProductId == productId && x.CompanyId == companyId
                     && x.TransactionType == TransactionType.Out && !x.IsDeleted)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        return stockIn - stockOut;
    }
}
