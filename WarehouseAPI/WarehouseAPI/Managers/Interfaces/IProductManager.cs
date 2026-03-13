using WarehouseAPI.DTOs.Product;

namespace WarehouseAPI.Managers.Interfaces;

public interface IProductManager
{
    Task<object> GetPagedAsync(ProductListRequestDto request);
    Task<ProductResponseDto?> GetByIdAsync(int id, string companyId);
    Task<(bool Success, string Message, ProductResponseDto? Data)> CreateAsync(CreateProductDto dto);
    Task<(bool Success, string Message)> UpdateAsync(UpdateProductDto dto);
    Task<(bool Success, string Message)> DeleteAsync(DeleteProductDto dto);
}
