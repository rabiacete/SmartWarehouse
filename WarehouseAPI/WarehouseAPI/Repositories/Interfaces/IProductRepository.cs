using WarehouseAPI.DTOs.Product;
using WarehouseAPI.Entities;

namespace WarehouseAPI.Repositories.Interfaces;

public interface IProductRepository
{
    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductListRequestDto request);
    Task<Product?> GetByIdAsync(int id, string companyId);
    Task<bool> SKUExistsAsync(string sku, string companyId, int? excludeId = null);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(int id, string companyId);
    Task<int> GetCurrentStockAsync(int productId, string companyId);
}
