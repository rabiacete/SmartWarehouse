using WarehouseAPI.DTOs.Product;
using WarehouseAPI.Entities;
using WarehouseAPI.Managers.Interfaces;
using WarehouseAPI.Repositories.Interfaces;

namespace WarehouseAPI.Managers;

public class ProductManager : IProductManager
{
    private readonly IProductRepository _repo;

    public ProductManager(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<object> GetPagedAsync(ProductListRequestDto request)
    {
        var (items, totalCount) = await _repo.GetPagedAsync(request);

        var dtos = new List<ProductResponseDto>();
        foreach (var item in items)
        {
            var stock = await _repo.GetCurrentStockAsync(item.Id, item.CompanyId);
            dtos.Add(MapToDto(item, stock));
        }

        return new
        {
            success = true,
            data = dtos,
            totalCount,
            page = request.Page,
            pageSize = request.PageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id, string companyId)
    {
        var product = await _repo.GetByIdAsync(id, companyId);
        if (product == null) return null;
        var stock = await _repo.GetCurrentStockAsync(id, companyId);
        return MapToDto(product, stock);
    }

    public async Task<(bool Success, string Message, ProductResponseDto? Data)> CreateAsync(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.", null);

        if (string.IsNullOrWhiteSpace(dto.ProductName) || string.IsNullOrWhiteSpace(dto.SKU))
            return (false, "Ürün adı ve SKU zorunludur.", null);

        if (await _repo.SKUExistsAsync(dto.SKU, dto.CompanyId))
            return (false, $"'{dto.SKU}' SKU kodu bu şirkette zaten kayıtlı.", null);

        var product = new Product
        {
            CompanyId = dto.CompanyId,
            ProductName = dto.ProductName,
            SKU = dto.SKU,
            Category = dto.Category,
            Unit = dto.Unit,
            Description = dto.Description,
            MinStockLevel = dto.MinStockLevel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(product);
        return (true, "Ürün başarıyla oluşturuldu.", MapToDto(created, 0));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.");

        var existing = await _repo.GetByIdAsync(dto.Id, dto.CompanyId);
        if (existing == null) return (false, "Ürün bulunamadı.");

        if (await _repo.SKUExistsAsync(dto.SKU, dto.CompanyId, dto.Id))
            return (false, $"'{dto.SKU}' SKU kodu bu şirkette zaten kayıtlı.");

        existing.ProductName = dto.ProductName;
        existing.SKU = dto.SKU;
        existing.Category = dto.Category;
        existing.Unit = dto.Unit;
        existing.Description = dto.Description;
        existing.MinStockLevel = dto.MinStockLevel;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(existing);
        return (true, "Ürün başarıyla güncellendi.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(DeleteProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyId))
            return (false, "CompanyId zorunludur.");

        var deleted = await _repo.SoftDeleteAsync(dto.Id, dto.CompanyId);
        if (!deleted) return (false, "Ürün bulunamadı.");

        return (true, "Ürün başarıyla silindi.");
    }

    private static ProductResponseDto MapToDto(Product p, int currentStock) => new()
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        ProductName = p.ProductName,
        SKU = p.SKU,
        Category = p.Category,
        Unit = p.Unit,
        Description = p.Description,
        MinStockLevel = p.MinStockLevel,
        CurrentStock = currentStock,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
