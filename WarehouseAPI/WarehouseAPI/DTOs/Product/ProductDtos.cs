namespace WarehouseAPI.DTOs.Product;

public class CreateProductDto
{
    public string CompanyId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinStockLevel { get; set; } = 0;
}

public class UpdateProductDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinStockLevel { get; set; } = 0;
}

public class DeleteProductDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
}

public class ProductListRequestDto
{
    public string CompanyId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
    public string? Category { get; set; }
}

public class ProductResponseDto
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinStockLevel { get; set; }
    public int CurrentStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
